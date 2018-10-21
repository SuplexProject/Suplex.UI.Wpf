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
            WebApiUrl = txtWebApiUrl.Text;
            DialogResult = true;
            Close();
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}