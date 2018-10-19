using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Suplex.Security.AclModel;
using Suplex.Security.DataAccess;

using Telerik.Windows.Controls;

namespace Suplex.UI.Wpf
{
    public partial class SecureObjectDlg : UserControl
    {
        SuplexStore _store = null;

        public SecureObjectDlg()
        {
            InitializeComponent();

            CurrentSecureObject = null;

            List<Type> rightTypes = EnumUtilities.GetRightTypes();
            cmdNewDaclAce.DropDownContent = rightTypes;
            cmdNewSaclAce.DropDownContent = rightTypes;
        }


        #region delete setup
        public override void OnApplyTemplate()
        {
            CommandBinding removeItem = new CommandBinding();
            removeItem.Command = ApplicationCommands.Delete;
            removeItem.Executed += DeleteItem_Command;
            CommandBindings.Add( removeItem );

            base.OnApplyTemplate();
        }

        private void DeleteItem_Command(object sender, ExecutedRoutedEventArgs e)
        {
            if( e.Parameter is SecureObject secureObject )
                DeleteSecureObject( secureObject );
        }
        #endregion


        #region public props
        public ISuplexDal SplxDal { get; set; } = null;

        public SuplexStore SplxStore
        {
            get => _store;
            set
            {
                if( _store != value )
                {
                    _store = value;

                    CurrentSecureObject = null;
                    DataContext = value?.SecureObjects;

                    ((GridViewComboBoxColumn)grdDacl.Columns["Groups"]).ItemsSource = value?.Groups;
                    ((GridViewComboBoxColumn)grdSacl.Columns["Groups"]).ItemsSource = value?.Groups;
                }
            }
        }
        #endregion


        #region new/delete
        private void cmdNewSecureObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item )
            {
                SecureObject secureObject = new SecureObject { UniqueName = "New Root" };

                if( item.Tag.ToString() == "Root" )
                    SplxStore.SecureObjects.Add( secureObject );
                else if( tvwSecureObjects.SelectedItem is SecureObject parent )
                {
                    secureObject.UniqueName = "New Child";
                    secureObject.ChangeParent( parent, SplxStore.SecureObjects ); ;
                }

                SplxDal.UpsertSecureObject( secureObject );

                listBox.SelectedItem = null;
                cmdNewSecureObject.IsOpen = false;
                tvwSecureObjects.Rebind();

                tvwSecureObjects.SelectedItem = secureObject;

                txtUniqueName.Focus();
                txtUniqueName.SelectAll();
            }
        }

        private void DeleteSecureObject(SecureObject secureObject)
        {
            if( secureObject != null )
            {
                if( secureObject.Parent == null )
                    SplxStore.SecureObjects.Remove( secureObject );
                else
                    secureObject.Parent.Children.Remove( secureObject );

                SplxDal.DeleteSecureObject( secureObject.UId );

                cmdDeleteSecureObject.IsOpen = false;
                tvwSecureObjects.Rebind();
            }
        }
        #endregion


        #region DataContext
        private SecureObject CachedSecureObject { get; set; }
        public SecureObject CurrentSecureObject
        {
            get { return pnlDetail.DataContext as SecureObject; }
            set
            {
                pnlDetail.DataContext = value;
                pnlDetail.IsEnabled = value != null;
                cmdDeleteSecureObject.IsEnabled = value != null;
            }
        }

        private void tvwSecureObjects_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            CachedSecureObject = tvwSecureObjects.SelectedItem as SecureObject;
            CloneCachedToCurrent();
        }
        void CloneCachedToCurrent()
        {
            CurrentSecureObject = CachedSecureObject?.Clone( shallow: false );
            CurrentSecureObject?.EnableIsDirty();

            cmdDeleteSecureObject.DropDownContent = CurrentSecureObject;
        }
        #endregion


        #region aces
        private void cmdNewDaclAce_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( sender is ListBox listBox && listBox.SelectedItem != null )
            {
                IAccessControlEntry ace =
                    AccessControlEntryUtilities.MakeGenericAceFromType( listBox.SelectedItem as Type );
                ace.UId = Guid.NewGuid();
                ace.SetRight( 1.ToString() );  //arbitrary minimum right

                CurrentSecureObject.Security.Dacl.Add( ace );

                grdDacl.SelectedItem = ace;
                grdDacl.Focus();

                listBox.SelectedItem = null;
                cmdNewDaclAce.IsOpen = false;
            }
        }

        private void cmdNewSaclAce_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( sender is ListBox listBox && listBox.SelectedItem != null )
            {
                IAccessControlEntryAudit ace =
                    AccessControlEntryUtilities.MakeGenericAceFromType( listBox.SelectedItem as Type, isAuditAce: true ) as IAccessControlEntryAudit;
                ace.UId = Guid.NewGuid();
                ace.SetRight( 1.ToString() );  //arbitrary minimum right

                CurrentSecureObject.Security.Sacl.Add( ace );

                grdSacl.SelectedItem = ace;
                grdSacl.Focus();

                listBox.SelectedItem = null;
                cmdNewSaclAce.IsOpen = false;
            }
        }

        // CurrentSecureObject.EnableIsDirty() doesn't cover updating an object in a collection,
        // this is a work-around to set IsDirty
        private void Acl_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            CurrentSecureObject.IsDirty = true;
        }
        #endregion


        #region save/discard
        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            SplxDal.UpsertSecureObject( CurrentSecureObject );
            CurrentSecureObject.IsDirty = false;
        }

        private void cmdDiscard_Click(object sender, RoutedEventArgs e)
        {
            CloneCachedToCurrent();
        }

        internal bool VerifySaveChanges()
        {
            return true;
        }

        internal void SaveIfDirty()
        {
        }
        #endregion
    }
}