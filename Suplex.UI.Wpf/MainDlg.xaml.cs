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
using Suplex.Security.Principal;
using Telerik.Windows.Controls;

namespace Suplex.UI.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainDlg : Window
    {
        public MainDlg()
        {
            StyleManager.ApplicationTheme = new Office2016Theme();
            InitializeComponent();

            #region foo
            List<User> users = new List<User>
            {
                new User{ Name = "x", IsBuiltIn = true, IsEnabled = true, IsLocal = true },
                new User{ Name = "y", IsBuiltIn = false, IsEnabled = true, IsLocal = false },
                new User{ Name = "z", IsBuiltIn = true, IsEnabled = false, IsLocal = true }
            };

            List<Group> groups = new List<Group>
            {
                new Group{ Name = "gx", IsEnabled = true, IsLocal = true },
                new Group{ Name = "gy", IsEnabled = true, IsLocal = false },
                new Group{ Name = "gz", IsEnabled = true, IsLocal = false }
            };
            bool isLocal = false;
            for( int i = 0; i < 50; i++ )
            {
                isLocal = !isLocal;
                groups.Add( new Group { Name = $"Group_{i}", IsLocal = isLocal } );
            }

            GroupMembershipItem mx = new GroupMembershipItem
            {
                GroupUId = groups[0].UId,
                MemberUId = users[0].UId,
                IsMemberUser = true
            };
            GroupMembershipItem my = new GroupMembershipItem
            {
                GroupUId = groups[0].UId,
                MemberUId = users[1].UId,
                IsMemberUser = true
            };
            GroupMembershipItem mz = new GroupMembershipItem
            {
                GroupUId = groups[0].UId,
                MemberUId = groups[1].UId,
                IsMemberUser = false
            };
            List<GroupMembershipItem> gm = new List<GroupMembershipItem>
            {
                mx, my, mz
            };


            SecureObject child = new SecureObject() { UniqueName = "child" };
            DiscretionaryAcl childdacl = new DiscretionaryAcl
            {
                new AccessControlEntry<FileSystemRight> { TrusteeUId = groups[0].UId, Allowed = true, Right = FileSystemRight.FullControl },
                new AccessControlEntry<FileSystemRight> { TrusteeUId = groups[1].UId, Allowed = false, Right = FileSystemRight.Execute | FileSystemRight.List, Inheritable = false },
                new AccessControlEntry<UIRight> { TrusteeUId = groups[2].UId, Right= UIRight.Operate | UIRight.Visible }
            };
            child.Security.Dacl = childdacl;


            SecureObject top = new SecureObject() { UniqueName = "top" };
            DiscretionaryAcl topdacl = new DiscretionaryAcl
            {
                new AccessControlEntry<FileSystemRight> { TrusteeUId = groups[0].UId, Allowed = true, Right = FileSystemRight.FullControl },
                new AccessControlEntry<FileSystemRight> { TrusteeUId = groups[1].UId, Allowed = false, Right = FileSystemRight.Execute | FileSystemRight.List, Inheritable = false },
                new AccessControlEntry<UIRight> { TrusteeUId = groups[2].UId, Right= UIRight.Operate | UIRight.Visible }
            };
            SystemAcl topsacl = new SystemAcl
            {
                new AccessControlEntryAudit<UIRight> { TrusteeUId = groups[0].UId, Allowed = true, Right = UIRight.FullControl, Inheritable = false },
                new AccessControlEntryAudit<FileSystemRight> { TrusteeUId = groups[1].UId, Allowed = true, Right = FileSystemRight.Execute | FileSystemRight.ReadPermissions, Inheritable = false },
                new AccessControlEntryAudit<FileSystemRight> { TrusteeUId = groups[2].UId, Allowed = true, Right = FileSystemRight.Execute | FileSystemRight.List, Inheritable = false }
            };
            top.Security.Dacl = topdacl;
            top.Security.DaclAllowInherit = false;
            top.Security.Sacl = topsacl;

            //child.ParentUId = top.UId;
            top.Children.Add( child );

            SecureObject top2 = new SecureObject() { UniqueName = "top2" };

            SecureObject grandkid = new SecureObject() { UniqueName = "grandkid" };
            child.Children.Add( grandkid );


            FileStore store = new FileStore()
            {
                SecureObjects = new List<SecureObject>() { top, top2 },
                Users = users,
                Groups = groups,
                GroupMembership = gm
            };

            for( int i = 0; i < 50; i++ )
                store.SecureObjects.Add( new SecureObject { UniqueName = $"UniqueName_{i}" } );

            //User ux = store.Users.GetByName<User>( "x" );

            string x = store.ToYaml();
            #endregion

            FileStore f = FileStore.FromYaml( x );

            f.SecureObjects.EnsureParentUIdRecursive();

            dlgSecureObjects.Store = f;
            dlgSecureObjects.SplxDal = f.Dal;

            dlgSecurityPrincipals.Store = f;
            dlgSecurityPrincipals.SplxDal = f.Dal;

            //dlgSecureObjects.DataContext = f.SecureObjects;
        }

        private void mnuRecentFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbSaveSplxFileStore_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbSaveSplxFileStoreSecure_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbNewSplxFileStore_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbOpenSplxFileStore_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbRemoteConnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbRemoteDisconnect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbRemoteRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbRemoteImport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbbRemoteExport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuRecentConnection_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}