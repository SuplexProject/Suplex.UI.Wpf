using System;
using System.Windows;

namespace Suplex.UI.Wpf
{
    public partial class ServiceConnectDlg : Window
    {
        public ServiceConnectDlg()
        {
            InitializeComponent();
        }

        public string WebApiUrl { get; set; }


        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            if( SuplexSecurityDalClient.ValidateServiceConnection( txtWebApiUrl.Text, out string exception ) )
            {
                WebApiUrl = txtWebApiUrl.Text;
                txtStatus.Text = $"Connected to {txtWebApiUrl.Text}!";
                DialogResult = true;
                Close();
            }
            else
                txtStatus.Text = exception;
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}