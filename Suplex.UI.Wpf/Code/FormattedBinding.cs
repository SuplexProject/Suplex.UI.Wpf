using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.ComponentModel;
using System.Linq;

//all of this code is stolen from Suplex
//  http://suplex.codeplex.com/
namespace Suplex.UI.Wpf
{
    //source derived from: http://learnwpf.com/Posts/Post.aspx?postId=05229e33-fcd4-44d5-9982-a002f2250a64
    //extended to include FormattedBinding
    //this class can be used with or without FormattedBinding
    //  ex: <Window.Resources><local:FormattingConverter x:Key="formatter"/></Window.Resources>
    //  ex: <TextBlock Text="{Binding Path=Id, Converter={StaticResource formatter}, ConverterParameter='Sale No:\{0\} '}" />
    //  ex: <TextBlock Text="{Binding Path=Amount, Converter={StaticResource formatter}, ConverterParameter=' \{0:C\}'}" FontWeight="Bold" />
    public class FormattingConverter : IValueConverter
    {
        private FormattedBinding _formattedBinding = null;

        public FormattingConverter() { }

        public FormattingConverter(FormattedBinding binding)
        {
            _formattedBinding = binding;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string formatString = parameter as string;
                if( formatString != null )
                {
                    if( _formattedBinding == null )
                    {
                        return string.Format( culture, formatString, value );
                    }
                    else
                    {
                        string valueAsString = value == null ? string.Empty : string.Format( "{0}", value );
                        return string.Format( culture, formatString,
                            string.IsNullOrEmpty( valueAsString ) ? _formattedBinding.ValueIfEmpty : value );
                    }
                }
                else
                {
                    string valueAsString = value == null ? string.Empty : string.Format( "{0}", value );
                    if( _formattedBinding == null )
                    {
                        return valueAsString;
                    }
                    else
                    {
                        return string.IsNullOrEmpty( valueAsString ) ? _formattedBinding.ValueIfEmpty : valueAsString;
                    }
                }
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        //we don't intend this to ever be called
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class EnumConverter : IValueConverter
    {
        private EnumFormattedBinding _formattedBinding = null;

        public EnumConverter(EnumFormattedBinding binding)
        {
            this._formattedBinding = binding;
        }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                object returnValue = this._formattedBinding.ValueIfEmpty;
                if( value != null )
                {
                    returnValue = value.ToString() == (string)this._formattedBinding.ValueIfTrue
                                      ? true
                                      : (value.ToString() == (string)this._formattedBinding.ValueIfFalse)
                                            ? false
                                            : true;
                }

                string formatString = parameter as string;
                if( formatString != null )
                {
                    returnValue = string.Format( culture, formatString, returnValue );
                }

                return returnValue;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }



    //source derived from: http://stackoverflow.com/questions/841808/wpf-display-a-bool-value-as-yes-no
    //renamed class, extended to include FormatString, ValueIfEmpty
    public class BooleanConverter : IValueConverter
    {
        private FormattedBinding _formattedBinding = null;

        public BooleanConverter(FormattedBinding binding)
        {
            this._formattedBinding = binding;
        }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                object returnValue = this._formattedBinding.ValueIfEmpty;
                if( value != null )
                {
                    bool b = System.Convert.ToBoolean( value );
                    if( this._formattedBinding.ValueIfTrue.ToString() == "Auto" )
                    {
                        this._formattedBinding.ValueIfTrue = Double.NaN;
                    }
                    returnValue = b ? this._formattedBinding.ValueIfTrue : this._formattedBinding.ValueIfFalse;
                }

                string formatString = parameter as string;
                if( formatString != null )
                {
                    returnValue = string.Format( culture, formatString, returnValue );
                }

                return returnValue;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
        #endregion
    }

    public class EnumFormattedBinding : Binding
    {
        private object _valueIfTrue = null;
        private object _valueIfFalse = null;
        private object _valueIfEmpty = null;
        private object _enumType = null;

        public EnumFormattedBinding()
            : base()
        {
            this.Initialize();
        }

        public EnumFormattedBinding(string path, object formatString)
            : this( path, Binding.DoNothing, Binding.DoNothing, formatString )
        {
        }

        public EnumFormattedBinding(string path, object valueIfTrue, object valueIfFalse)
            : this( path, valueIfTrue, valueIfFalse, Binding.DoNothing )
        {
        }

        public EnumFormattedBinding(string path, object valueIfTrue, object valueIfFalse, object formatString)
            : this( path )
        {
            this.ValueIfTrue = valueIfTrue;
            this.ValueIfFalse = valueIfFalse;
            this.FormatString = formatString;
            Converter = Converter ?? new EnumConverter( this );
            Mode = BindingMode.TwoWay;
        }

        public EnumFormattedBinding(string path)
            : base( path )
        {
            this.Initialize();
        }
        private void Initialize()
        {
            this._enumType = Binding.DoNothing;
            this._valueIfTrue = Binding.DoNothing;
            this._valueIfFalse = Binding.DoNothing;
            this.FormatString = Binding.DoNothing;
            //Converter = new FormattingConverter(this);
        }

        [ConstructorArgument( "valueIfTrue" )]
        public object ValueIfTrue
        {
            get { return this._valueIfTrue; }
            set
            {
                this._valueIfTrue = value;
                Converter = Converter ?? new EnumConverter( this );
            }
        }

        [ConstructorArgument( "valueIfFalse" )]
        public object ValueIfFalse
        {
            get { return this._valueIfFalse; }
            set
            {
                this._valueIfFalse = value;
                Converter = Converter ?? new EnumConverter( this );
            }
        }

        [ConstructorArgument( "valueIfEmpty" )]
        public object ValueIfEmpty
        {
            get { return this._valueIfEmpty; }
            set
            {
                this._valueIfEmpty = value;
                Converter = Converter ?? new EnumConverter( this );
            }
        }

        [ConstructorArgument( "enumType" )]
        public object EnumType
        {
            get { return this._enumType; }
            set
            {
                this._enumType = value;
                Converter = Converter ?? new EnumConverter( this );
            }
        }

        [ConstructorArgument( "formatString" )]
        public object FormatString
        {
            get { return ConverterParameter; }
            set { ConverterParameter = value; }
        }

    }


    //source derived from: http://stackoverflow.com/questions/841808/wpf-display-a-bool-value-as-yes-no
    //renamed class, extended to include FormatString, ValueIfEmpty, and multiple Converters
    //  ex: Text="{local:FormattedBinding Path=Name, FormatString='Object Name: \{0\}'}"
    public class FormattedBinding : Binding
    {
        private object _valueIfTrue = null;
        private object _valueIfFalse = null;
        private object _valueIfEmpty = null;

        public FormattedBinding()
            : base()
        {
            this.Initialize();
        }

        public FormattedBinding(string path)
            : base( path )
        {
            this.Initialize();
        }

        public FormattedBinding(string path, object formatString)
            : this( path )
        {
            this.FormatString = formatString;
        }

        public FormattedBinding(string path, object valueIfTrue, object valueIfFalse)
            : this( path )
        {
            this.ValueIfTrue = valueIfTrue;
            this.ValueIfFalse = valueIfFalse;
            this.Converter = new BooleanConverter( this );
        }

        public FormattedBinding(string path, object valueIfTrue, object valueIfFalse, object formatString)
            : this( path )
        {
            this.ValueIfTrue = valueIfTrue;
            this.ValueIfFalse = valueIfFalse;
            this.FormatString = formatString;
            this.Converter = new BooleanConverter( this );
        }

        private void Initialize()
        {
            _valueIfTrue = Binding.DoNothing;
            _valueIfFalse = Binding.DoNothing;
            this.FormatString = Binding.DoNothing;
            this.Converter = new FormattingConverter( this );
        }

        [ConstructorArgument( "valueIfTrue" )]
        public object ValueIfTrue
        {
            get { return _valueIfTrue; }
            set
            {
                _valueIfTrue = value;
                this.Converter = new BooleanConverter( this );
            }
        }

        [ConstructorArgument( "valueIfFalse" )]
        public object ValueIfFalse
        {
            get { return _valueIfFalse; }
            set
            {
                _valueIfFalse = value;
                this.Converter = new BooleanConverter( this );
            }
        }

        [ConstructorArgument( "valueIfEmpty" )]
        public object ValueIfEmpty { get { return _valueIfEmpty; } set { _valueIfEmpty = value; } }

        [ConstructorArgument( "formatString" )]
        public object FormatString { get { return this.ConverterParameter; } set { this.ConverterParameter = value; } }
    }

    public class EventArgs<T> : EventArgs
    {
        public EventArgs() : base() { }
        public EventArgs(T eventData)
            : base()
        {
            this.EventData = eventData;
        }
        public T EventData { get; internal set; }
    }


    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;


        public EnumerationExtension(Type enumType)
        {
            if( enumType == null )
            {
                throw new ArgumentNullException( "enumType" );
            }

            this.EnumType = enumType;
        }

        public Type EnumType
        {
            get { return this._enumType; }
            private set
            {
                if( this._enumType == value )
                {
                    return;
                }

                Type enumType = Nullable.GetUnderlyingType( value ) ?? value;

                if( enumType.IsEnum == false )
                {
                    throw new ArgumentException( "Type must be an Enum." );
                }

                this._enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Array enumValues = Enum.GetValues( this.EnumType );

            return (
                       from object enumValue in enumValues
                       select new EnumerationMember
                       {
                           Value = enumValue,
                           Description = this.GetDescription( enumValue )
                       }).ToArray();
        }

        private string GetDescription(object enumValue)
        {
            DescriptionAttribute descriptionAttribute = this.EnumType
                                                            .GetField( enumValue.ToString() )
                                                            .GetCustomAttributes( typeof( DescriptionAttribute ), false )
                                                            .FirstOrDefault() as DescriptionAttribute;


            return descriptionAttribute != null
                       ? descriptionAttribute.Description
                       : enumValue.ToString();
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }



    public static class UiUtilities
    {
        public static System.Windows.Visibility BoolToVisibility(bool value)
        {
            return value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }
        public static System.Windows.Visibility BoolToVisibility(bool value, System.Windows.Visibility falseValue)
        {
            return value ? System.Windows.Visibility.Visible : falseValue;
        }
        public static System.Windows.Visibility BoolToVisibility(bool value, System.Windows.Visibility trueValue, System.Windows.Visibility falseValue)
        {
            return value ? trueValue : falseValue;
        }

        public static string ConvertToTitleCase(string value)
        {
            System.Globalization.TextInfo ti = new System.Globalization.CultureInfo( "en-US", false ).TextInfo;
            string result = ti.ToTitleCase( value );
            return result.Replace( "And", "and" ).Replace( "Or", "or" ).Replace( "_", " " );
        }

        public static T GetChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int i = 0;
            bool found = false;
            DependencyObject child = null;
            while( i < VisualTreeHelper.GetChildrenCount( parent ) && !found )
            {
                child = VisualTreeHelper.GetChild( parent, i );
                found = child != null && child.GetType() == typeof( T );
                if( !found )
                {
                    child = GetChild<T>( child );
                    found = child != null && child.GetType() == typeof( T );
                }
                i++;
            }

            return child as T;
        }

        //public static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        //{
        //    while( source != null && source.GetType() != typeof( T ) )
        //        source = VisualTreeHelper.GetParent( source );

        //    return source;
        //}
        public static T VisualUpwardSearch<T>(DependencyObject source) where T : class
        {
            while( source != null && source.GetType() != typeof( T ) )
                source = VisualTreeHelper.GetParent( source );

            return source as T;
        }
        //public static TreeViewItem VisualUpwardSearch(DependencyObject source)
        //{
        //    while( source != null && !(source is TreeViewItem) )
        //        source = VisualTreeHelper.GetParent( source );

        //    return source as TreeViewItem;
        //}

        public static FrameworkElement FindByName(string name, FrameworkElement root)
        {
            if( root == null ) { return null; }

            Stack<FrameworkElement> tree = new Stack<FrameworkElement>();
            tree.Push( root );

            while( tree.Count > 0 )
            {
                FrameworkElement current = tree.Pop();
                if( current.Name == name )
                    return current;

                int count = VisualTreeHelper.GetChildrenCount( current );
                for( int i = 0; i < count; ++i )
                {
                    DependencyObject child = VisualTreeHelper.GetChild( current, i );
                    if( child is FrameworkElement )
                        tree.Push( (FrameworkElement)child );
                }
            }

            return null;
        }
    }
}
