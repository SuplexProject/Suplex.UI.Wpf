using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
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
        IDataAccessLayer _splxDal = null;
        SuplexStore _splxStore = null;
        FileStore _fileStore = null;

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

            _fileStore = FileStore.FromYaml( x );

            _fileStore.SecureObjects.EnsureParentUIdRecursive();

            dlgSecureObjects.SplxStore = _fileStore;
            dlgSecureObjects.SplxDal = _fileStore.Dal;

            dlgSecurityPrincipals.SplxStore = _fileStore;
            dlgSecurityPrincipals.SplxDal = _fileStore.Dal;

            //dlgSecureObjects.DataContext = f.SecureObjects;
        }

        #region file
        private void tbbNewSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            bool ok = GlobalVerifySaveChanges();
            if( ok )
            {
                FileNew();
            }
        }

        private void tbbOpenSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            bool ok = GlobalVerifySaveChanges();
            if( ok )
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = "Suplex Files|*.splx;*.xml|All Files|*.*"
                };
                if( dlg.ShowDialog( this ) == true )
                    OpenFile( dlg.FileName );
            }
        }

        private void mnuRecentFile_Click(object sender, RoutedEventArgs e)
        {
            bool ok = GlobalVerifySaveChanges();
            if( ok )
            {
                string file = ((MenuItem)e.OriginalSource).Header.ToString();
                if( File.Exists( file ) )
                {
                    OpenFile( file );
                }
            }
        }

        private void tbbSaveSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            SetMainDlgDataContext();
        }

        private void tbbSaveAsSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
            SetMainDlgDataContext();
        }

        private void tbbSaveSplxFileStoreSecure_Click(object sender, RoutedEventArgs e)
        {
            //if( _fileExportDlg == null )
            //{
            //    _fileExportDlg = new FileExportDlg();
            //    _fileExportDlg.Owner = this;
            //    _fileExportDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //}

            //_fileExportDlg.IsForExport = false;
            //if( _fileExportDlg.ShowDialog() == true )
            //{
            //    _apiClient.SetFile( _fileExportDlg.FileName );
            //    if( _fileExportDlg.SignFile )
            //    {
            //        _apiClient.SetPublicPrivateKeyFile( _fileExportDlg.KeysFileName );
            //        _apiClient.PublicPrivateKeyContainerName = _fileExportDlg.KeysContainerName;
            //    }
            //    else
            //    {
            //        _apiClient.SetPublicPrivateKeyFile( null );
            //        _apiClient.PublicPrivateKeyContainerName = null;
            //    }
            //    SaveFile();
            //}
        }

        private void tbbSaveAllSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            dlgSecureObjects.SaveIfDirty();
            dlgSecurityPrincipals.SaveIfDirty();

            //if( !_splxDal.IsConnected )
            //{
            //    SaveFile();
            //}
        }

        private void FileNew()
        {
            _fileStore = new FileStore();
            _splxStore = _fileStore;
            _splxDal = _fileStore.Dal;

            //_splxStore.Clear();
            //_splxDal.SetFile( null );
            //_splxStore.IsDirty = false;

            //dlgSecureObjects.ClearContentPanel();
            //dlgSecurityPrincipals.ClearContentPanel();

            SetMainDlgDataContext();
        }

        private void OpenFile(string fileName)
        {
            FileNew();

            _fileStore = FileStore.FromYamlFile( fileName );
            _splxStore = _fileStore;
            _splxDal = _fileStore.Dal;
            _fileStore.SecureObjects.EnsureParentUIdRecursive();

            SetMainDlgDataContext();

            //_settings.AddRecentFile( fileName );    //re-sorts the recent file list

            ////_splx.GroupMembership.Resolve();
        }

        private bool SaveFile()
        {
            bool ok = _fileStore.CurrentPath != null;
            if( !ok )
            {
                ok = SaveFileAs();
            }
            else
            {
                //_splxDal.SaveFile( _splxStore );
                _fileStore.ToYamlFile();
                ok = true;
            }

            return ok;
        }

        private bool SaveFileAs()
        {
            bool ok = false;
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "Suplex File|*.splx|Suplex XML File|*.xml"
            };
            if( dlg.ShowDialog( this ) == true )
            {
                _fileStore.ToYamlFile( dlg.FileName );

                ok = true;   //SaveFile();
            }

            return ok;
        }
        #endregion


        private bool StoreFileVerifySaveChanges()
        {
            bool ok = false;
            //if( !_splxDal.IsConnected && _splxStore.IsDirty )
            //{
            //    MessageBoxResult mbr =
            //        MessageBox.Show( string.Format( "Save changes to {0}?", _splxDal.HasFileConnection ? _splxDal.File.Name : "Untitled Document" ),
            //        "Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes );

            //    switch( mbr )
            //    {
            //        case MessageBoxResult.Yes:
            //        {
            //            ok = SaveFile();
            //            break;
            //        }
            //        case MessageBoxResult.No:
            //        {
            //            ok = true;
            //            break;
            //        }
            //        case MessageBoxResult.Cancel:
            //        {
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    //no item to verify or item is not dirty
            //    ok = true;
            //}

            //return ok;

            return true;
        }

        private bool GlobalVerifySaveChanges()
        {
            bool ok = dlgSecureObjects.VerifySaveChanges();
            if( ok )
                ok = dlgSecurityPrincipals.VerifySaveChanges();
            if( ok )
                ok = StoreFileVerifySaveChanges();

            return ok;
        }

        private void SetMainDlgDataContext()
        {
            if( DataContext == null )
                DataContext = new DialogViewModel();

            ((DialogViewModel)DataContext).ConnectionPath = _fileStore.CurrentPath;

            dlgSecureObjects.SplxStore = _splxStore;
            dlgSecureObjects.SplxDal = _splxDal;

            dlgSecurityPrincipals.SplxStore = _splxStore;
            dlgSecurityPrincipals.SplxDal = _splxDal;
        }

        //private void mnuRecentFile_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void tbbSaveSplxFileStore_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void tbbSaveSplxFileStoreSecure_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void tbbNewSplxFileStore_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void tbbOpenSplxFileStore_Click(object sender, RoutedEventArgs e)
        //{

        //}

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