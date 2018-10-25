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
using System.Windows.Shapes;

using Suplex.Security.AclModel;
using Suplex.Security.WebApi;

namespace Suplex.UI.Wpf
{
    /// <summary>
    /// Interaction logic for ServiceConnectDlg.xaml
    /// </summary>
    public partial class ServiceConnectDlg : Window
    {
        public ServiceConnectDlg()
        {
            InitializeComponent();
        }

        public string WebApiUrl { get; set; }


        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SuplexSecurityHttpApiClient apiClient = new SuplexSecurityHttpApiClient( txtWebApiUrl.Text, configureAwaitContinueOnCapturedContext: false );
                //this is just a connection test, doesn't matter what the result is
                SecureObject secureObject = apiClient.GetSecureObjectByUId( Guid.NewGuid(), includeChildren: false ) as SecureObject;

                WebApiUrl = txtWebApiUrl.Text;
                DialogResult = true;
                Close();
            }
            catch( Exception ex )
            {
                this.Title = ex.Message;
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}