using System;
using System.Windows;
using System.Windows.Data;


namespace Suplex.UI.Wpf
{
    public class LocalGroupVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if( values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue )
                return null;

            bool isUser = (bool)values[0];
            bool isLocal = (bool)values[1];

            return !isUser && isLocal ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}