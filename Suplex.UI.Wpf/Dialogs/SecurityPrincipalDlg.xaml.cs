using Suplex.Security.AclModel.DataAccess;
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

                    DataContext = value?.Groups;
                }
            }
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmdDiscard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmdNewPrincipal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmdDeletePrincipal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
