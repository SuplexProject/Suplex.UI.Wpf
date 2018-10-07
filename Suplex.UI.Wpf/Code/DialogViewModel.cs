using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;


namespace Suplex.UI.Wpf
{
    public class DialogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string _connectionPath = null;
        public virtual string ConnectionPath
        {
            get => _connectionPath;
            set
            {
                if( value != _connectionPath )
                {
                    _connectionPath = value;
                    IsDirty = true;
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( ConnectionPath ) ) );
                }
            }
        }

        bool _isDirty = false;
        public virtual bool IsDirty
        {
            get => _isDirty;
            set
            {
                if( value != _isDirty )
                {
                    _isDirty = value;
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( IsDirty ) ) );
                }
            }
        }

        public override string ToString()
        {
            return ConnectionPath;
        }
    }
}