using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Suplex.Security.AclModel;

namespace Suplex.UI.Wpf
{
    public partial class AceConverterDlg : UserControl
    {
        public event EventHandler Created;

        public AceConverterDlg()
        {
            InitializeComponent();

            List<Type> rightTypes = EnumUtilities.GetRightTypes();
            lstSourceType.DataContext = rightTypes;
            lstTargetType.DataContext = rightTypes;

            SourceRightType = rightTypes[0];
            TargetRightType = rightTypes[0];
        }

        public Type SourceRightType { get; private set; }
        public Type TargetRightType { get; private set; }

        //todo: binding would be better, but being lazy today
        private void lstSourceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SourceRightType = (Type)lstSourceType.SelectedItem;
        }

        private void lstTargetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TargetRightType = (Type)lstTargetType.SelectedItem;
        }

        private void cmdCreate_Click(object sender, RoutedEventArgs e)
        {
            Created?.Invoke( this, EventArgs.Empty );
        }
    }
}