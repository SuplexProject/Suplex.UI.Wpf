using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;

using Suplex.Utilities.Serialization;
using YamlDotNet.Serialization;

namespace Suplex.UI.Wpf
{
    public class SuplexMru
    {
        public SuplexMru()
        {
            RecentFiles = new ObservableCollection<string>();
            RecentServices = new ObservableCollection<string>();
            RecentDatabases = new ObservableCollection<DatabaseConnectionData>();

            CollectionContainer rsc = new CollectionContainer() { Collection = RecentServices };
            CollectionContainer rdc = new CollectionContainer() { Collection = RecentDatabases };

            RecentRemoteConnections = new CompositeCollection
            {
                rsc,
                rdc
            };
        }

        public ObservableCollection<string> RecentFiles { get; set; }
        public ObservableCollection<string> RecentServices { get; set; }
        [YamlIgnore()]
        public ObservableCollection<DatabaseConnectionData> RecentDatabases { get; set; }
        //public System.Drawing.Point FileToolbar { get; set; }
        //public System.Drawing.Point DatabaseToolbar { get; set; }
        //public System.Drawing.Point ViewToolbar { get; set; }

        [YamlIgnore()]
        public CompositeCollection RecentRemoteConnections { get; private set; }

        [YamlIgnore()]
        private static string FileName
        {
            get
            {
                return string.Format( @"{0}\Suplex.UI.Wpf.Mru.yaml",
                    Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location ) );
            }
        }


        public void AddRecentFile(string file)
        {
            if( !string.IsNullOrEmpty( file ) )
            {
                for( int i = RecentFiles.Count - 1; i >= 0; i-- )
                {
                    if( RecentFiles[i].ToString() == file )
                    {
                        RecentFiles.Remove( file );
                        break;
                    }
                }
                RecentFiles.Insert( 0, file );
            }
        }

        public void AddRecentService(string url)
        {
            if( !string.IsNullOrEmpty( url ) )
            {
                for( int i = RecentServices.Count - 1; i >= 0; i-- )
                {
                    if( RecentServices[i].ToString() == url )
                    {
                        RecentServices.Remove( url );
                        break;
                    }
                }
                RecentServices.Insert( 0, url );
            }
        }

        public void AddRecentDatabase(string server, string db, string username)
        {
            if( !string.IsNullOrEmpty( server ) )
            {
                DatabaseConnectionData cd = new DatabaseConnectionData()
                {
                    Server = server,
                    Database = db
                };

                if( !string.IsNullOrEmpty( username ) )
                    cd.UserName = username;

                AddRecentDatabase( cd );
            }
        }
        public void AddRecentDatabase(DatabaseConnectionData connectionData)
        {
            for( int i = RecentDatabases.Count - 1; i >= 0; i-- )
            {
                if( RecentDatabases[i].ToString() == connectionData.ToString() )
                {
                    RecentDatabases.RemoveAt( i );
                    break;
                }
            }
            RecentDatabases.Insert( 0, connectionData );
        }

        public void Serialize()
        {
            YamlHelpers.SerializeFile( FileName, this );
        }

        public static SuplexMru Deserialize()
        {
            if( File.Exists( FileName ) )
                return YamlHelpers.DeserializeFile<SuplexMru>( FileName );
            else
                return new SuplexMru();
        }
    }

    public class DatabaseConnectionData : INotifyPropertyChanged
    {
        private string _server;
        private string _database;
        private string _username;
        private string _password;

        [XmlAttribute()]
        public string Server
        {
            get { return _server; }
            set
            {
                if( value != _server )
                {
                    _server = value;
                    OnPropertyChanged( "Server" );
                }
            }
        }

        [XmlAttribute()]
        public string Database
        {
            get { return _database; }
            set
            {
                if( value != _database )
                {
                    _database = value;
                    OnPropertyChanged( "Database" );
                }
            }
        }

        [XmlAttribute()]
        public string UserName
        {
            get { return _username; }
            set
            {
                if( value != _username )
                {
                    _username = value;
                    OnPropertyChanged( "UserName" );
                }
            }
        }

        [XmlIgnore()]
        public string Password
        {
            get { return _password; }
            set
            {
                if( value != _password )
                {
                    _password = value;
                    OnPropertyChanged( "Password" );
                }
            }
        }

        [XmlIgnore()]
        public bool UseSqlCredentials { get { return !string.IsNullOrEmpty( UserName ); } }

        public override string ToString()
        {
            string username = string.Empty;
            if( UseSqlCredentials )
            {
                username = string.Format( " [{0}]", UserName );
            }
            return string.Format( @"{0} :: {1}{2}", Server, Database, username );
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }
        #endregion
    }
}