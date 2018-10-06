using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Suplex.UI.Wpf
{
    public partial class FilterTextBox : UserControl
    {
        public event TextChangedEventHandler TextChanged;

        protected void OnTextChanged(TextChangedEventArgs e)
        {
            TextChanged?.Invoke( txtFilter, e );
        }


        public FilterTextBox()
        {
            InitializeComponent();
        }

        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.Key == Key.Escape )
            {
                txtFilter.Clear();
                txtFilter.Focus();
                e.Handled = true;
            }
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFilter.Visibility = txtFilter.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
            OnTextChanged( e );
        }

        private void cmdClear_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtFilter.Clear();
            txtFilter.Focus();
        }

        public string Text { get { return txtFilter.Text; } }
    }
}