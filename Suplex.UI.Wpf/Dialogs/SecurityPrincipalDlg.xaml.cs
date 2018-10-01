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

        private ISecurityPrincipal CachedSecureObject { get; set; }
        public ISecurityPrincipal CurrentSecureObject
        {
            get { return pnlDetail.DataContext as ISecurityPrincipal; }
            set
            {
                pnlDetail.DataContext = value;
                pnlDetail.IsEnabled = value != null;
            }
        }

        private void grdPrincipals_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            CachedSecureObject = grdPrincipals.SelectedItem as ISecurityPrincipal;
            CloneCachedToCurrent();
        }
        void CloneCachedToCurrent()
        {
            CurrentSecureObject = CachedSecureObject;
            //CurrentSecureObject = CachedSecureObject?.Clone( shallow: false );
            //CurrentSecureObject?.EnableIsDirty();
            //cmdDeleteSecureObject.DropDownContent = new List<SecureObject> { CurrentSecureObject };
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
