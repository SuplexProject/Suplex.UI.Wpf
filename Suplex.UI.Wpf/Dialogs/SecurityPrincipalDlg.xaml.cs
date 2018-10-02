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

using Suplex.Security.AclModel.DataAccess;
using Suplex.Security.Principal;

using Telerik.Windows.Controls;


namespace Suplex.UI.Wpf
{
    /// <summary>
    /// Interaction logic for SecurityPrincipalDlg.xaml
    /// </summary>
    public partial class SecurityPrincipalDlg : UserControl
    {
        SuplexStore _store = null;

        public SecurityPrincipalDlg()
        {
            InitializeComponent();

            CurrentSecurityPrincipal = null;
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

                    DataContext = value?.Groups;
                    txtGroupLookup.ItemsSource = value?.Groups;
                }
            }
        }

        private SecurityPrincipalBase CachedSecurityPrincipal { get; set; }
        public SecurityPrincipalBase CurrentSecurityPrincipal
        {
            get { return pnlDetail.DataContext as SecurityPrincipalBase; }
            set
            {
                pnlDetail.DataContext = value;
                pnlDetail.IsEnabled = value != null;
            }
        }

        private void grdPrincipals_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            CachedSecurityPrincipal = grdPrincipals.SelectedItem as SecurityPrincipalBase;
            CloneCachedToCurrent();
        }
        void CloneCachedToCurrent()
        {
            CurrentSecurityPrincipal = CachedSecurityPrincipal?.Clone( shallow: false ) as SecurityPrincipalBase;
            CurrentSecurityPrincipal?.EnableIsDirty();
            cmdDeletePrincipal.DropDownContent = new List<SecurityPrincipalBase> { CurrentSecurityPrincipal };

            IEnumerable<GroupMembershipItem> items = SplxDal.GetGroupMembers( CurrentSecurityPrincipal.UId, includeDisabledMembership: true );
            items.Resolve( Store.Groups, Store.Users );
            lstGroupMembership.DataContext = items;
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            SplxDal.UpsertGroup( CurrentSecurityPrincipal as Group );
            CurrentSecurityPrincipal.IsDirty = false;
        }

        private void cmdDiscard_Click(object sender, RoutedEventArgs e)
        {
            CloneCachedToCurrent();
        }

        private void cmdNewPrincipal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmdDeletePrincipal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmdAddGroupMembers_Click(object sender, RoutedEventArgs e)
        {
            foreach( Group g in txtGroupLookup.SelectedItems )
                lstGroupMembership.Items.Add( g );

            txtGroupLookup.SelectedItems = null;
        }
    }
}
