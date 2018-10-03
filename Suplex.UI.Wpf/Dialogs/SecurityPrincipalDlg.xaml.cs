using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            CurrentSecurityPrincipalMembership = new ObservableCollection<GroupMembershipItemWrapper>();
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

                    Principals = new CompositeCollection
                    {
                        new CollectionContainer{ Collection = value.Users },
                        new CollectionContainer{ Collection = value.Groups }
                    };
                    PrincipalsCvs = new CollectionViewSource
                    {
                        Source = Principals
                    };

                    DataContext = PrincipalsCvs.View;
                    txtGroupLookup.ItemsSource = value?.Groups;
                }
            }
        }

        public CompositeCollection Principals { get; set; }
        public CollectionViewSource PrincipalsCvs { get; set; }


        #region data context
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
        public ObservableCollection<GroupMembershipItemWrapper> CurrentSecurityPrincipalMembership
        {
            get { return lstGroupMembership.DataContext as ObservableCollection<GroupMembershipItemWrapper>; }
            set { lstGroupMembership.DataContext = value; }
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

            CurrentSecurityPrincipalMembership.Clear();

            if( CurrentSecurityPrincipal is Group group && group.IsLocal )
            {
                IEnumerable<GroupMembershipItem> groupMembershipItems = SplxDal.GetGroupMembers( CurrentSecurityPrincipal.UId, includeDisabledMembership: true );
                foreach( GroupMembershipItem item in groupMembershipItems )
                {
                    item.Resolve( Store.Groups, Store.Users );
                    CurrentSecurityPrincipalMembership.Add( new GroupMembershipItemWrapper( item, true ) );
                }
            }
            else if( CurrentSecurityPrincipal is User user )
            {
                IEnumerable<GroupMembershipItem> groupMembershipItems = SplxDal.GetGroupMembership( CurrentSecurityPrincipal.UId, includeDisabledMembership: true );
                foreach( GroupMembershipItem item in groupMembershipItems )
                {
                    item.Resolve( Store.Groups, Store.Users );
                    CurrentSecurityPrincipalMembership.Add( new GroupMembershipItemWrapper( item, false ) );
                }
            }
        }
        #endregion


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
            GroupMembershipItemWrapper item = null;

            foreach( Group g in txtGroupLookup.SelectedItems )
                if( CurrentSecurityPrincipal is Group )
                    CurrentSecurityPrincipalMembership.Add(
                        new GroupMembershipItemWrapper( new GroupMembershipItem( CurrentSecurityPrincipal as Group, g ), true ) );
                else
                    CurrentSecurityPrincipalMembership.Add(
                        new GroupMembershipItemWrapper( new GroupMembershipItem( g, CurrentSecurityPrincipal ), false ) );


            txtGroupLookup.SelectedItems = null;
        }
    }


    //world's cheapest excuse for a view model:
    public class GroupMembershipItemWrapper : GroupMembershipItem
    {
        public GroupMembershipItemWrapper(GroupMembershipItem item, bool displayMember)
        {
            GroupUId = item.GroupUId;
            MemberUId = item.MemberUId;
            IsMemberUser = item.IsMemberUser;

            //these two Props are Fields on the base class, need them to be props for databinding
            GroupItem = item.Group;
            MemberItem = item.Member;

            DisplayItem = displayMember ? item.Member : item.Group;

            BaseItem = item;
        }

        public Group GroupItem { get; set; }
        public SecurityPrincipalBase MemberItem { get; set; }
        public SecurityPrincipalBase DisplayItem { get; set; }
        public GroupMembershipItem BaseItem { get; set; }
    }
}