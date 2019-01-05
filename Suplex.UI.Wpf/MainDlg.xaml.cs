using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using Telerik.Windows.Controls;

namespace Suplex.UI.Wpf
{
    public partial class MainDlg : Window
    {
        SuplexMru _mru = null;
        SuplexSecurityDalClient _dal = null;
        readonly string _fileFilter = "Suplex Files|*.splx;*.xml|All Files|*.*";

        public MainDlg()
        {
            StyleManager.ApplicationTheme = new Office2016Theme(); //Expression_DarkTheme
            InitializeComponent();

            _dal = new SuplexSecurityDalClient();
            DataContext = _dal;
            dlgSecureObjects.SplxDal = _dal;
            dlgSecurityPrincipals.SplxDal = _dal;

            LoadMru();

            FileNew();

            //note: when compiling SuplexAdmin as a dll, comment the below lines and remove App.xaml/App.xaml.cs from project
            //best to backup/restore SuplexApp.csproj before/after compiling as dll, VisualStudio can sometimes fruit with project file in wierd ways
            //comment from here:
            if( App.StartUpDocumentIsValid )
            {
                OpenFile( App.StartUpDocument );
            }
            else if( App.CommandLineArgs.Count > 0 )
            {
                if( App.CommandLineArgs.Keys.Contains( "/config" ) )
                {
                    //placeholder
                    //this.OpenConfig( App.CommandLineArgs["/config"] );
                }
                else if( App.CommandLineArgs.Keys.Contains( "/dbserver" ) && App.CommandLineArgs.Keys.Contains( "/dbname" ) )
                {
                    if( App.CommandLineArgs.Keys.Contains( "/dbuser" ) && App.CommandLineArgs.Keys.Contains( "/dbpswd" ) )
                    {
                        //placeholder
                    }
                }
            }
            //:to here
        }

        #region MainDlg handlers, Startup/Shutdown
        private void MainDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if( GlobalVerifySaveChanges() )
                _mru.Serialize();
            else
                e.Cancel = true;
        }

        private void LoadMru()
        {
            _mru = SuplexMru.Deserialize();
            tbbOpenSplxFileStore.DropDownContextMenu.DataContext = _mru.RecentFiles;
            tbbRemoteConnect.DropDownContextMenu.DataContext = _mru.RecentServices;
        }
        #endregion

        #region file
        private void tbbNewSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            if( GlobalVerifySaveChanges() )
                FileNew();
        }

        private void tbbOpenSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            if( GlobalVerifySaveChanges() )
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = _fileFilter 
                };
                if( dlg.ShowDialog( this ).Value )
                    OpenFile( dlg.FileName );
            }
        }

        private void mnuRecentFile_Click(object sender, RoutedEventArgs e)
        {
            if( GlobalVerifySaveChanges() )
            {
                string file = ((MenuItem)e.OriginalSource).Header.ToString();
                if( File.Exists( file ) )
                    OpenFile( file );
            }
        }

        private void tbbSaveSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void tbbSaveAsSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs( _dal.AsFileSystemDal );
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
            _dal.InitFileSystemDal( null, false );
        }

        private void OpenFile(string fileName)
        {
            _dal.InitFileSystemDal( fileName, autoSave: false );
            _mru.AddRecentFile( fileName );
        }

        void SaveFile()
        {
            if( !_dal.HasConnectionPath )
                SaveFileAs( _dal.AsFileSystemDal );
            else
                _dal.AsFileSystemDal.ToYamlFile();
        }

        void SaveFileAs(FileSystemDal fileSystemDal)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "Suplex File|*.splx|Suplex YAML File|*.yaml"
            };
            if( dlg.ShowDialog( this ).Value )
                fileSystemDal.ToYamlFile( dlg.FileName );   //make sure that sets the file context
        }
        #endregion

        #region service
        private void tbbRemoteConnect_Click(object sender, RoutedEventArgs e)
        {
            ServiceConnectDlg dlg = new ServiceConnectDlg
            {
                ShowInTaskbar = false,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if( dlg.ShowDialog().Value )
                _dal.InitWebApiConnection( dlg.WebApiUrl );

            _mru.AddRecentService( dlg.WebApiUrl );
        }

        private void tbbRemoteDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _dal.RehostDalToFileSystemDal();
        }

        private void tbbRemoteRefresh_Click(object sender, RoutedEventArgs e)
        {
            _dal.RefreshStore();
        }

        private void tbbRemoteImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = _fileFilter
            };
            if( dlg.ShowDialog( this ).Value )
            {
                FileSystemDal file = FileSystemDal.LoadFromYamlFile( dlg.FileName );
                _dal.ImportSuplexObjects( file );
                _dal.RefreshStore();
            }
        }

        private void tbbRemoteExport_Click(object sender, RoutedEventArgs e)
        {
            FileSystemDal export = new FileSystemDal()
            {
                Store = _dal.Store
            };
            export.Store.GroupMembership =
                new System.Collections.Generic.List<Security.Principal.GroupMembershipItem>( _dal.GetGroupMembership() );

            SaveFileAs( export );
        }

        private void mnuRecentConnection_Click(object sender, RoutedEventArgs e)
        {
            if( GlobalVerifySaveChanges() )
            {
                string url = ((MenuItem)e.OriginalSource).Header.ToString();
                if( SuplexSecurityDalClient.ValidateServiceConnection( url, out string exception ) )
                    _dal.InitWebApiConnection( url );
                else
                    MessageBox.Show( exception, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation );
            }

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
    }
}