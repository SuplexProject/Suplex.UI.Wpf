using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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


        #region ctor
        public SecurityPrincipalDlg()
        {
            InitializeComponent();

            CurrentSecurityPrincipal = null;
            CurrentSecurityPrincipalMemberOf = new ObservableCollection<GroupMembershipItemWrapper>();
            CurrentSecurityPrincipalMembers = new ObservableCollection<GroupMembershipItemWrapper>();
            txtGroupMemberOfLookup.SelectedItems = null;
            txtGroupMemberOfLookup.SearchText = string.Empty;
            txtGroupMembersLookup.SelectedItems = null;
            txtGroupMembersLookup.SearchText = string.Empty;
        }
        #endregion


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
            if( e.Parameter is SecurityPrincipalBase securityPrincipal )
                DeletePrincipal( securityPrincipal );
            else if( e.Parameter is GroupMembershipItemWrapper gmi )
                DeleteGroupMembershipItem( gmi );
        }
        #endregion


        #region public props
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

                    txtGroupMembersLookup.ItemsSource = AllPrincipalsCvs.View;
                    txtGroupMemberOfLookup.ItemsSource = LocalGroupsCvs.View;
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
        #endregion


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

        public ObservableCollection<GroupMembershipItemWrapper> CurrentSecurityPrincipalMemberOf
        {
            get { return lstGroupMemberOf.DataContext as ObservableCollection<GroupMembershipItemWrapper>; }
            set { lstGroupMemberOf.DataContext = value; }
        }

        public ObservableCollection<GroupMembershipItemWrapper> CurrentSecurityPrincipalMembers
        {
            get { return lstGroupMembers.DataContext as ObservableCollection<GroupMembershipItemWrapper>; }
            set { lstGroupMembers.DataContext = value; }
        }

        List<GroupMembershipItemWrapper> CurrentSecurityPrincipalDeletedMembership { get; set; } = new List<GroupMembershipItemWrapper>();

        private void grdPrincipals_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            CachedSecurityPrincipal = grdPrincipals.SelectedItem as SecurityPrincipalBase;
            CloneCachedToCurrent();
        }
        void CloneCachedToCurrent()
        {
            CurrentSecurityPrincipalMemberOf.Clear();
            CurrentSecurityPrincipalMembers.Clear();
            CurrentSecurityPrincipalDeletedMembership.Clear();
            txtGroupMemberOfLookup.SelectedItems = null;
            txtGroupMemberOfLookup.SearchText = string.Empty;
            txtGroupMembersLookup.SelectedItems = null;
            txtGroupMembersLookup.SearchText = string.Empty;


            CurrentSecurityPrincipal = CachedSecurityPrincipal?.Clone( shallow: false ) as SecurityPrincipalBase;

            cmdDeletePrincipal.DropDownContent = CurrentSecurityPrincipal;

            if( CurrentSecurityPrincipal != null )
            {
                CurrentSecurityPrincipal.EnableIsDirty();

                IEnumerable<GroupMembershipItem> groupMemberOf = SplxDal.GetGroupMemberOf( CurrentSecurityPrincipal.UId, includeDisabledMembership: true );
                foreach( GroupMembershipItem item in groupMemberOf )
                {
                    item.Resolve( Store.Groups, Store.Users );
                    CurrentSecurityPrincipalMemberOf.Add( new GroupMembershipItemWrapper( item, displayMember: false ) );
                }

                if( CurrentSecurityPrincipal is Group group && group.IsLocal )
                {
                    IEnumerable<GroupMembershipItem> groupMembershipItems = SplxDal.GetGroupMembers( CurrentSecurityPrincipal.UId, includeDisabledMembership: true );
                    foreach( GroupMembershipItem item in groupMembershipItems )
                    {
                        item.Resolve( Store.Groups, Store.Users );
                        CurrentSecurityPrincipalMembers.Add( new GroupMembershipItemWrapper( item, displayMember: true ) );
                    }
                }
            }
        }
        #endregion


        #region new/delete securityPrincipal
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

                txtPrincipalName.Focus();
                txtPrincipalName.SelectAll();
            }
        }

        private void DeletePrincipal(SecurityPrincipalBase securityPrincipal)
        {
            if( securityPrincipal != null )
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

                AllPrincipalsCvs.View.Refresh();
                cmdDeletePrincipal.IsOpen = false;
            }
        }
        #endregion


        #region group membership
        private void cmdAddGroupMemberOf_Click(object sender, RoutedEventArgs e)
        {
            GroupMembershipItemWrapper item = null;

            foreach( SecurityPrincipalBase sp in txtGroupMemberOfLookup.SelectedItems )
            {
                if( CurrentSecurityPrincipal.UId != sp.UId )
                {
                    item = new GroupMembershipItemWrapper( new GroupMembershipItem( sp as Group, CurrentSecurityPrincipal ), displayMember: false );

                    if( !CurrentSecurityPrincipalMemberOf.ContainsItem( item ) )
                    {
                        CurrentSecurityPrincipalMemberOf.Add( item );
                        CurrentSecurityPrincipal.IsDirty = true;
                    }
                }
            }

            txtGroupMemberOfLookup.SelectedItems = null;
            txtGroupMemberOfLookup.SearchText = string.Empty;
        }

        private void cmdAddGroupMembers_Click(object sender, RoutedEventArgs e)
        {
            GroupMembershipItemWrapper item = null;

            foreach( SecurityPrincipalBase sp in txtGroupMembersLookup.SelectedItems )
            {
                if( CurrentSecurityPrincipal.UId != sp.UId )
                {
                    item = new GroupMembershipItemWrapper( new GroupMembershipItem( CurrentSecurityPrincipal as Group, sp ), displayMember: true );

                    if( !CurrentSecurityPrincipalMembers.ContainsItem( item ) )
                    {
                        CurrentSecurityPrincipalMembers.Add( item );
                        CurrentSecurityPrincipal.IsDirty = true;
                    }
                }
            }

            txtGroupMembersLookup.SelectedItems = null;
            txtGroupMembersLookup.SearchText = string.Empty;
        }

        private void DeleteGroupMembershipItem(GroupMembershipItemWrapper gmi)
        {
            if( gmi != null )
            {
                int index = CurrentSecurityPrincipalMembers.IndexOf( gmi );
                if( index > -1 )
                    CurrentSecurityPrincipalMembers.RemoveAt( index );
                else
                    CurrentSecurityPrincipalMemberOf.Remove( gmi );

                CurrentSecurityPrincipalDeletedMembership.Add( gmi );

                CurrentSecurityPrincipal.IsDirty = true;
            }
        }
        #endregion


        #region save/discard
        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            if( CurrentSecurityPrincipal.IsUser )
                SplxDal.UpsertUser( CurrentSecurityPrincipal as User );
            else
                SplxDal.UpsertGroup( CurrentSecurityPrincipal as Group );

            //process deletes before adds in case a deleted item is re-added
            foreach( GroupMembershipItemWrapper gm in CurrentSecurityPrincipalDeletedMembership )
                SplxDal.DeleteGroupMembership( gm );
            foreach( GroupMembershipItemWrapper gm in CurrentSecurityPrincipalMemberOf )
                SplxDal.UpsertGroupMembership( gm );
            foreach( GroupMembershipItemWrapper gm in CurrentSecurityPrincipalMembers )
                SplxDal.UpsertGroupMembership( gm );

            AllPrincipalsCvs.View.Refresh();
            LocalGroupsCvs.View.Refresh();

            CurrentSecurityPrincipal.IsDirty = false;
        }

        private void cmdDiscard_Click(object sender, RoutedEventArgs e)
        {
            CloneCachedToCurrent();
        }
        #endregion
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