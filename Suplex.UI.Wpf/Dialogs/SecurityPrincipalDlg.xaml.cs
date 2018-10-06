using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Suplex.Security.AclModel.DataAccess;
using Suplex.Security.Principal;

using Telerik.Windows.Controls;


namespace Suplex.UI.Wpf
{
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

        CollectionViewSource _usersCvs_Filtered = null;
        CollectionViewSource _groupsCvs_Filtered = null;
        CollectionViewSource _usersCvs_Unfiltered = null;
        CollectionViewSource _groupsCvs_Unfiltered = null;
        CollectionViewSource _allPrincipalsCvs_Filtered = null;
        CollectionViewSource _allPrincipalsCvs_Unfiltered = null;
        CollectionViewSource _localGroupsCvs = null;

        void RefreshViews()
        {
            _usersCvs_Filtered.View.Refresh();
            _groupsCvs_Filtered.View.Refresh();
            _allPrincipalsCvs_Filtered.View.Refresh();
            _allPrincipalsCvs_Unfiltered.View.Refresh();
            _localGroupsCvs.View.Refresh();
        }

        public SuplexStore Store
        {
            get => _store;
            set
            {
                if( _store != value )
                {
                    _store = value;

                    //note: must add SortDescriptions *before* View.Filter or filter doesn't work
                    _usersCvs_Filtered = new CollectionViewSource { Source = _store.Users };
                    _usersCvs_Filtered.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
                    _usersCvs_Filtered.View.Filter = SecurityPrincipalFilter;
                    _groupsCvs_Filtered = new CollectionViewSource { Source = _store.Groups };
                    _groupsCvs_Filtered.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
                    _groupsCvs_Filtered.View.Filter = SecurityPrincipalFilter;
                    _allPrincipalsCvs_Filtered = new CollectionViewSource
                    {
                        Source = new CompositeCollection
                        {
                            new CollectionContainer{ Collection = _usersCvs_Filtered.View },
                            new CollectionContainer{ Collection = _groupsCvs_Filtered.View }
                        }
                    };

                    DataContext = _allPrincipalsCvs_Filtered.View;


                    _usersCvs_Unfiltered = new CollectionViewSource { Source = _store.Users };
                    _usersCvs_Unfiltered.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
                    _groupsCvs_Unfiltered = new CollectionViewSource { Source = _store.Groups };
                    _groupsCvs_Unfiltered.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ) );
                    _allPrincipalsCvs_Unfiltered = new CollectionViewSource
                    {
                        Source = new CompositeCollection
                        {
                            new CollectionContainer{ Collection = _usersCvs_Unfiltered.View },
                            new CollectionContainer{ Collection = _groupsCvs_Unfiltered.View }
                        }
                    };

                    txtGroupMembersLookup.ItemsSource = _allPrincipalsCvs_Unfiltered.View;


                    _localGroupsCvs = new CollectionViewSource { Source = _store.Groups };
                    _localGroupsCvs.View.Filter = item =>
                    {
                        if( !(item is Group g) ) return false;
                        if( !g.IsLocal ) return false; //!g.IsEnabled || 
                        return true;
                    };

                    txtGroupMemberOfLookup.ItemsSource = _localGroupsCvs.View;
                }
            }
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _usersCvs_Filtered.View.Refresh();
            _groupsCvs_Filtered.View.Refresh();
            _allPrincipalsCvs_Filtered.View.Refresh();
        }
        private bool SecurityPrincipalFilter(object item)
        {
            if( !(item is SecurityPrincipalBase sp) ) return false;
            return sp.Name.IndexOf( txtFilter.Text, StringComparison.OrdinalIgnoreCase ) >= 0;
        }
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
                List<GroupMembershipItemWrapper> tmp = new List<GroupMembershipItemWrapper>();
                foreach( GroupMembershipItem item in groupMemberOf )
                {
                    item.Resolve( Store.Groups, Store.Users );
                    tmp.Add( new GroupMembershipItemWrapper( item, displayMember: false ) );
                }
                tmp.Sort( (x, y) => x.GroupItem.Name.CompareTo( y.GroupItem.Name ) );
                CurrentSecurityPrincipalMemberOf = new ObservableCollection<GroupMembershipItemWrapper>( tmp );

                tmp.Clear();
                if( CurrentSecurityPrincipal is Group group && group.IsLocal )
                {
                    IEnumerable<GroupMembershipItem> groupMembers = SplxDal.GetGroupMembers( CurrentSecurityPrincipal.UId, includeDisabledMembership: true );
                    foreach( GroupMembershipItem item in groupMembers )
                    {
                        item.Resolve( Store.Groups, Store.Users );
                        tmp.Add( new GroupMembershipItemWrapper( item, displayMember: true ) );
                    }
                    tmp.Sort( (x, y) => x.MemberItem.Name.CompareTo( y.MemberItem.Name ) );
                    CurrentSecurityPrincipalMembers = new ObservableCollection<GroupMembershipItemWrapper>( tmp );
                }

                tmp = null;
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

                RefreshViews();
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

                RefreshViews();
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

            //refresh the display
            CurrentSecurityPrincipalMemberOf =
                new ObservableCollection<GroupMembershipItemWrapper>( CurrentSecurityPrincipalMemberOf.OrderBy( gmi => gmi.GroupItem.Name ) );
            CurrentSecurityPrincipalMembers =
                new ObservableCollection<GroupMembershipItemWrapper>( CurrentSecurityPrincipalMembers.OrderBy( gmi => gmi.MemberItem.Name ) );

            RefreshViews();

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