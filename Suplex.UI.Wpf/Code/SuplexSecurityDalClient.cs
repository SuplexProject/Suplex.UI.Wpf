using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Suplex.Security.AclModel;
using Suplex.Security.DataAccess;
using Suplex.Security.Principal;
using Suplex.Security.WebApi;


namespace Suplex.UI.Wpf
{
    public class SuplexSecurityDalClient : ISuplexDal, INotifyPropertyChanged
    {
        ISuplexDal _dal = null;

        public SuplexSecurityDalClient()
        {
        }

        public void InitFileSystemDal(string filePath, bool autoSave)
        {
            _dal = new FileSystemDal();
            if( !string.IsNullOrWhiteSpace( filePath ) )
                AsFileSystemDal.Configure( new FileSystemDalConfig { FilePath = filePath, AutomaticallyPersistChanges = autoSave } );
            ConnectionPath = filePath;
            IsServiceConnection = false;

            RefreshStore();
        }
        public void InitWebApiConnection(string baseUrl, string messageFormatType = "application/json")
        {
            _dal = new SuplexSecurityHttpApiClient( baseUrl, messageFormatType, configureAwaitContinueOnCapturedContext: false );
            ConnectionPath = baseUrl;
            IsServiceConnection = true;

            RefreshStore();
        }

        public void RehostDalToFileSystemDal()
        {
            List<GroupMembershipItem> gm = new List<GroupMembershipItem>( _dal.GetGroupMembership() ); ;
            _dal = new FileSystemDal();
            AsFileSystemDal.Store = Store;
            AsFileSystemDal.Store.GroupMembership = gm;
            IsServiceConnection = false;
            ConnectionPath = null;
        }

        public FileSystemDal AsFileSystemDal { get { return (FileSystemDal)_dal; } }

        public void RefreshStore()
        {
            if( IsServiceConnection )
            {
                Store = new SuplexStore
                {
                    Users = _dal.GetUserByName( null, false ),
                    Groups = _dal.GetGroupByName( null, false ),
                    SecureObjects = _dal.GetSecureObjects() as List<SecureObject>
                };
            }
            else
            {
                if( HasConnectionPath )
                    AsFileSystemDal.FromYamlFile( ConnectionPath );
                Store = AsFileSystemDal.Store as SuplexStore;
            }

            Store.SecureObjects?.EnsureParentUIdRecursive();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        SuplexStore _store = null;
        public virtual SuplexStore Store
        {
            get => _store;
            set
            {
                if( value != _store )
                {
                    _store = value;
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( Store ) ) );
                }
            }
        }

        bool _isConnected = false;
        public virtual bool IsServiceConnection
        {
            get => _isConnected;
            set
            {
                if( value != _isConnected )
                {
                    _isConnected = value;
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( IsServiceConnection ) ) );
                }
            }
        }

        string _connectionPath = null;
        public virtual string ConnectionPath
        {
            get => _connectionPath;
            set
            {
                if( value != _connectionPath )
                {
                    _connectionPath = value;
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( ConnectionPath ) ) );
                }
            }
        }
        public bool HasConnectionPath { get { return !string.IsNullOrWhiteSpace( _connectionPath ); } }
        #endregion

        #region utilities
        public static bool ValidateServiceConnection(string url, out string exception)
        {
            exception = null;
            try
            {
                //this is just a connection test, doesn't matter what the result is
                SecureObject secureObject = new SuplexSecurityHttpApiClient( url, configureAwaitContinueOnCapturedContext: false )
                    .GetSecureObjectByUId( Guid.NewGuid(), includeChildren: false ) as SecureObject;
            }
            catch( Exception ex )
            {
                exception = ExceptionHelpers.UnwindException( ex );
            }

            return exception == null;
        }

        public void ImportSuplexObjects(FileSystemDal source)
        {
            foreach( User user in source.Store.Users )
                UpsertUser( user );
            foreach( Group group in source.Store.Groups )
                UpsertGroup( group );
            foreach( GroupMembershipItem gmi in source.Store.GroupMembership )
                UpsertGroupMembership( gmi );
            RecurseSecureObjectsForImport( source.Store.SecureObjects );
        }
        void RecurseSecureObjectsForImport(IList<SecureObject> list)
        {
            foreach( SecureObject secureObject in list )
            {
                UpsertSecureObject( secureObject );
                RecurseSecureObjectsForImport( secureObject.Children );
            }
        }
        #endregion

        #region ISuplexDal
        public User GetUserByUId(Guid userUId)
        {
            return _dal.GetUserByUId( userUId );
        }
        public List<User> GetUserByName(string name, bool exact = false)
        {
            return _dal.GetUserByName( name, exact );
        }
        public User UpsertUser(User user)
        {
            return _dal.UpsertUser( user );
        }
        public void DeleteUser(Guid userUId)
        {
            _dal.DeleteUser( userUId );
        }

        public Group GetGroupByUId(Guid groupUId)
        {
            return _dal.GetGroupByUId( groupUId );
        }
        public List<Group> GetGroupByName(string name, bool exact = false)
        {
            return _dal.GetGroupByName( name, exact );
        }
        public Group UpsertGroup(Group group)
        {
            return _dal.UpsertGroup( group );
        }
        public void DeleteGroup(Guid groupUId)
        {
            _dal.DeleteGroup( groupUId );
        }

        public IEnumerable<GroupMembershipItem> GetGroupMembership()
        {
            return _dal.GetGroupMembership();
        }
        public IEnumerable<GroupMembershipItem> GetGroupMembers(Guid groupUId, bool includeDisabledMembership = false)
        {
            return _dal.GetGroupMembers( groupUId, includeDisabledMembership );
        }
        public IEnumerable<GroupMembershipItem> GetGroupMemberOf(Guid memberUId, bool includeDisabledMembership = false)
        {
            return _dal.GetGroupMemberOf( memberUId, includeDisabledMembership );
        }
        public IEnumerable<GroupMembershipItem> GetGroupMembershipHierarchy(Guid memberUId, bool includeDisabledMembership = false)
        {
            return _dal.GetGroupMembershipHierarchy( memberUId, includeDisabledMembership );
        }
        public GroupMembershipItem UpsertGroupMembership(GroupMembershipItem groupMembershipItem)
        {
            return _dal.UpsertGroupMembership( groupMembershipItem );
        }
        public List<GroupMembershipItem> UpsertGroupMembership(List<GroupMembershipItem> groupMembershipItems)
        {
            return _dal.UpsertGroupMembership( groupMembershipItems );
        }
        public void DeleteGroupMembership(GroupMembershipItem groupMembershipItem)
        {
            _dal.DeleteGroupMembership( groupMembershipItem );
        }

        public MembershipList<SecurityPrincipalBase> GetGroupMembersList(Guid groupUId, bool includeDisabledMembership = false)
        {
            return _dal.GetGroupMembersList( groupUId, includeDisabledMembership );
        }
        public MembershipList<SecurityPrincipalBase> GetGroupMembersList(Group group, bool includeDisabledMembership = false)
        {
            return _dal.GetGroupMembersList( group, includeDisabledMembership );
        }
        public MembershipList<Group> GetGroupMemberOfList(Guid memberUId, bool isMemberGroup = false, bool includeDisabledMembership = false)
        {
            return _dal.GetGroupMemberOfList( memberUId, isMemberGroup, includeDisabledMembership );
        }
        public MembershipList<Group> GetGroupMemberOfList(SecurityPrincipalBase member, bool includeDisabledMembership = false)
        {
            return _dal.GetGroupMemberOfList( member, includeDisabledMembership );
        }

        public IEnumerable<ISecureObject> GetSecureObjects()
        {
            return _dal.GetSecureObjects();
        }
        public ISecureObject GetSecureObjectByUId(Guid secureObjectUId, bool includeChildren, bool includeDisabled = false)
        {
            return _dal.GetSecureObjectByUId( secureObjectUId, includeChildren, includeDisabled );
        }
        public ISecureObject GetSecureObjectByUniqueName(string uniqueName, bool includeChildren, bool includeDisabled = false)
        {
            return _dal.GetSecureObjectByUniqueName( uniqueName, includeChildren, includeDisabled );
        }
        public ISecureObject UpsertSecureObject(ISecureObject secureObject)
        {
            //if the Parent property is set the object won't serialize for a WebApi call
            ISecureObject clone = secureObject.Clone();
            clone.Parent = null;
            return _dal.UpsertSecureObject( clone );
        }
        public void UpdateSecureObjectParentUId(ISecureObject secureObject, Guid? newParentUId)
        {
            _dal.UpdateSecureObjectParentUId( secureObject, newParentUId );
        }
        public void DeleteSecureObject(Guid secureObjectUId)
        {
            _dal.DeleteSecureObject( secureObjectUId );
        }
        #endregion
    }
}