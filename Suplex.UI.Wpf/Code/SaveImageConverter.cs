using System;
using System.Windows;
using System.Windows.Data;


namespace Suplex.UI.Wpf
{
    public class SaveImageConverter : IMultiValueConverter
    {
        //expected in "parameter":
        //0: Resource Path (ex: "Resources/ToolBar/file/")
        //1: color regular save icon
        //2: color save_secure icon
        //3: grey regular save icon
        //4: grey save_secure icon
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] parts = parameter.ToString().Split( ',' );

            if( values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue )
                return new System.Windows.Media.Imaging.BitmapImage(
                    new Uri( string.Format( "{0}{1}", parts[0], parts[1] ), UriKind.Relative ) );

            string imageName;

            bool isEnabled = (bool)values[0];
            bool isSecure = (bool)values[1];

            if( isEnabled )
                imageName = !isSecure ? parts[1] : parts[2];
            else
                imageName = !isSecure ? parts[3] : parts[4];


            return new System.Windows.Media.Imaging.BitmapImage(
                new Uri( string.Format( "{0}{1}", parts[0], imageName ), UriKind.Relative ) );
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}