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
using Suplex.Security.DataAccess;
using Suplex.Security.Principal;
using Telerik.Windows.Controls;

namespace Suplex.UI.Wpf
{
    public partial class MainDlg : Window
    {
        SuplexSecurityDalClient _dal = null;

        public MainDlg()
        {
            StyleManager.ApplicationTheme = new Office2016Theme(); //Expression_DarkTheme
            InitializeComponent();

            _dal = new SuplexSecurityDalClient();
            DataContext = _dal;
            dlgSecureObjects.SplxDal = _dal;
            dlgSecurityPrincipals.SplxDal = _dal;

            FileNew();
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
        }

        private void tbbSaveAsSplxFileStore_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
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
        }

        void SaveFile()
        {
            if( !_dal.HasConnectionPath )
                SaveFileAs();
            else
                _dal.AsFileSystemDal.ToYamlFile();
        }

        void SaveFileAs()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "Suplex File|*.splx|Suplex XML File|*.xml"
            };
            if( dlg.ShowDialog( this ).Value )
                _dal.AsFileSystemDal.ToYamlFile( dlg.FileName );   //SaveFile();  <-- todo: make sure that sets the file context
        }
        #endregion

        #region service
        private void tbbRemoteConnect_Click(object sender, RoutedEventArgs e)
        {
            ServiceConnectDlg dlg = new ServiceConnectDlg();
            dlg.ShowInTaskbar = false;
            if( dlg.ShowDialog().Value )
                _dal.InitWebApiConnection( dlg.WebApiUrl );
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

        }

        private void tbbRemoteExport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuRecentConnection_Click(object sender, RoutedEventArgs e)
        {

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