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
using Suplex.Security.AclModel.DataAccess;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.DragDrop;

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

            DragDropManager.AddDragDropCompletedHandler( tvwSecureObjects, tvwSecureObjects_DropCompleted );
        }

        public IDataAccessLayer SplxDal { get; set; } = null;

        public SuplexStore Store
        {
            get => _store;
            set
            {
                if( _store != value )
                {
                    _store = value;

                    DataContext = value?.SecureObjects;

                    ((GridViewComboBoxColumn)grdDacl.Columns["Groups"]).ItemsSource = value?.Groups;
                    ((GridViewComboBoxColumn)grdSacl.Columns["Groups"]).ItemsSource = value?.Groups;
                }
            }
        }

        public SecureObject CurrentSecureObject { get { return pnlDetail.DataContext as SecureObject; } set { pnlDetail.DataContext = value; } }

        private void tvwSecureObjects_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            SecureObject secureObject = tvwSecureObjects.SelectedItem as SecureObject;
            CurrentSecureObject = secureObject?.Clone( shallow: false );
        }

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

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            SplxDal.UpsertSecureObject( CurrentSecureObject );
        }

        #region drag/drop
        private void tvwSecureObjects_DropCompleted(object sender, DragDropCompletedEventArgs e)
        {
            //e.
        }
        #endregion
    }
}