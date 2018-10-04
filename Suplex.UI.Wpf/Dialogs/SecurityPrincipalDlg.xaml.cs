using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

                    AllPrincipalsCvs = new CollectionViewSource
                    {
                        Source = new CompositeCollection
                        {
                            new CollectionContainer{ Collection = _store.Users },
                            new CollectionContainer{ Collection = _store.Groups }
                        }
                        //,CollectionViewType = typeof( ListCollectionView )
                    };
                    //AllPrincipalsCvs.View.Filter = item =>
                    //{
                    //    if( !(item is SecurityPrincipalBase sp) ) return false;
                    //    if( !sp.IsEnabled ) return false;
                    //    return true;
                    //};

                    LocalGroupsCvs = new CollectionViewSource { Source = _store.Groups };
                    LocalGroupsCvs.View.Filter = item =>
                    {
                        if( !(item is Group g) ) return false;
                        if( !g.IsLocal ) return false; //!g.IsEnabled || 
                        return true;
                    };

                    DataContext = AllPrincipalsCvs.View;
                    txtGroupLookup.ItemsSource = null;
                }
            }
        }
        private bool UserFilter(object item)
        {
            if( !(item is SecurityPrincipalBase sp) ) return false;
            if( !sp.IsEnabled ) return false;
            return true;
        }

        public CollectionViewSource AllPrincipalsCvs { get; set; }
        public CollectionViewSource LocalGroupsCvs { get; set; }


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

                AllPrincipalsCvs.View.Refresh();
                txtGroupLookup.ItemsSource = AllPrincipalsCvs.View;
            }
            else if( CurrentSecurityPrincipal is User user )
            {
                IEnumerable<GroupMembershipItem> groupMembershipItems = SplxDal.GetGroupMembership( CurrentSecurityPrincipal.UId, includeDisabledMembership: true );
                foreach( GroupMembershipItem item in groupMembershipItems )
                {
                    item.Resolve( Store.Groups, Store.Users );
                    CurrentSecurityPrincipalMembership.Add( new GroupMembershipItemWrapper( item, false ) );
                }

                LocalGroupsCvs.View.Refresh();
                txtGroupLookup.ItemsSource = LocalGroupsCvs.View;
            }
        }
        #endregion


        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            if( CurrentSecurityPrincipal.IsUser )
                SplxDal.UpsertUser( CurrentSecurityPrincipal as User );
            else
                SplxDal.UpsertGroup( CurrentSecurityPrincipal as Group );

            foreach( GroupMembershipItemWrapper gm in CurrentSecurityPrincipalMembership )
                SplxDal.UpsertGroupMembership( gm );

            CurrentSecurityPrincipal.IsDirty = false;
        }

        private void cmdDiscard_Click(object sender, RoutedEventArgs e)
        {
            CloneCachedToCurrent();
        }

        private void cmdNewPrincipal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item )
            {
                SecurityPrincipalBase sp = null;
                if( item.Tag.ToString() == "User" )
                {
                    User user = SplxDal.UpsertUser( new User { Name = "New User" } );
                    sp = user;
                    if( !Store.Users.Contains( user ) )
                        Store.Users.Add( user );
                }
                else
                {
                    Group group = SplxDal.UpsertGroup( new Group { Name = "New Group" } );
                    sp = group;
                    if( !Store.Groups.Contains( group ) )
                        Store.Groups.Add( group );
                }

                AllPrincipalsCvs.View.Refresh();
                listBox.SelectedItem = null;
                cmdNewPrincipal.IsOpen = false;
                grdPrincipals.SelectedItem = sp;
            }
        }

        private void cmdDeletePrincipal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( sender is ListBox listBox && listBox.SelectedItem is SecurityPrincipalBase securityPrincipal )
            {
                if( securityPrincipal.IsUser )
                {
                    Store.Users.Remove( securityPrincipal as User );
                    SplxDal.DeleteUser( securityPrincipal.UId );
                }
                else
                {
                    Store.Groups.Remove( securityPrincipal as Group );
                    SplxDal.DeleteGroup( securityPrincipal.UId );
                }

                listBox.SelectedItem = null;
                cmdNewPrincipal.IsOpen = false;
            }
        }

        private void cmdAddGroupMembers_Click(object sender, RoutedEventArgs e)
        {
            GroupMembershipItemWrapper item = null;

            foreach( Group g in txtGroupLookup.SelectedItems )
            {
                if( CurrentSecurityPrincipal is Group )
                    item = new GroupMembershipItemWrapper( new GroupMembershipItem( CurrentSecurityPrincipal as Group, g ), true );
                else
                    item = new GroupMembershipItemWrapper( new GroupMembershipItem( g, CurrentSecurityPrincipal ), false );

                if( !CurrentSecurityPrincipalMembership.ContainsItem( item ) )
                {
                    CurrentSecurityPrincipalMembership.Add( item );
                    CurrentSecurityPrincipal.IsDirty = true;
                }
            }

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