using System;
using System.Windows;
using System.Windows.Data;


namespace Suplex.UI.Wpf
{
    public class SecurityPrincipalTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if( values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue )
                return null;

            string[] images = parameter.ToString().Split( ',' );
            bool isUser = (bool)values[0];
            bool isLocal = (bool)values[1];

            string path = images[0];
            if( isUser )
                path += images[1];
            else
            {
                if( isLocal )
                    path += images[2];
                else
                    path += images[3];
            }

            Uri uriSource = new Uri( path, UriKind.Relative );
            return new System.Windows.Media.Imaging.BitmapImage( uriSource ); ;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}