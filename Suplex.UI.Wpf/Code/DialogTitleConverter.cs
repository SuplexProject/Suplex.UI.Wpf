using System;
using System.Windows;
using System.Windows.Data;


namespace Suplex.UI.Wpf
{
    public class DialogTitleConverter : IMultiValueConverter
    {
        //expected in "parameter":
        //0: Main Application Title (ex: "Suplex")
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string connectionPath =
                values[0] == DependencyProperty.UnsetValue ? string.Empty : values[0] as string;
            connectionPath = string.IsNullOrWhiteSpace( connectionPath ) ? string.Empty :
                string.Format( ": {0}", values[0].ToString() );


            //string dataSourceName = string.Empty;

            //bool isConnected = (bool)values[0];
            //string databaseConnectionString = values[1] == DependencyProperty.UnsetValue ? string.Empty : values[1].ToString();
            //string filePath = values[2] == DependencyProperty.UnsetValue ? string.Empty : values[2].ToString();
            //bool isDirty = (bool)values[3];

            //if( isConnected )
            //{
            //    if( !string.IsNullOrEmpty( databaseConnectionString ) )
            //    {
            //        dataSourceName = string.Format( ": {0}", databaseConnectionString );
            //    }
            //}
            //else
            //{
            //    if( !string.IsNullOrEmpty( filePath ) )
            //    {
            //        dataSourceName = string.Format( ": {0}{1}", filePath, isDirty ? "*" : "" );
            //    }
            //}

            return string.Format( "{0}{1}", parameter, connectionPath );
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}