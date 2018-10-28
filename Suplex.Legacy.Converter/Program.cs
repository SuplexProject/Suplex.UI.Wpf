using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Data;

using ssa = Suplex.Security.AclModel;
using ssp = Suplex.Security.Principal;

using legacySplxSecurity = Suplex.Security;
using legacySplxApi = Suplex.Forms.ObjectModel.Api;

namespace Suplex.Legacy.Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            if( args.Length == 2 && File.Exists( args[0] ) )
                Convert( args[0], args[1] );
            else
                Console.WriteLine( "Syntax: Suplex.Legacy.Converter sourceLegacyFileName upgradeOutputFilename" );
        }

        public static void Convert(string legacyFile, string upgradeFile)
        {
            legacySplxApi.SuplexApiClient legacyApi = new legacySplxApi.SuplexApiClient();
            legacySplxApi.SuplexStore legacyStore = legacyApi.LoadFile( legacyFile );

            FileSystemDal fsd = new FileSystemDal
            {
                CurrentPath = upgradeFile
            };

            foreach( legacySplxApi.User legacyUser in legacyStore.Users )
                fsd.Store.Users.Add( legacyUser.ToNewUser() );
            foreach( legacySplxApi.Group legacyGroup in legacyStore.Groups )
                fsd.Store.Groups.Add( legacyGroup.ToNewGroup() );
            foreach( legacySplxApi.GroupMembershipItem legacyGmi in legacyStore.GroupMembership.InnerList.Values )
                fsd.Store.GroupMembership.Add( legacyGmi.ToNewGroupMembership() );

            RecurseSecureObjectsForImport( legacyStore.UIElements, fsd.Store.SecureObjects );

            fsd.ToYamlFile();
        }

        static void RecurseSecureObjectsForImport(IEnumerable<legacySplxApi.UIElement> uiElements, IList<ssa.SecureObject> secureObjects)
        {
            foreach( legacySplxApi.UIElement uie in uiElements )
            {
                ssa.SecureObject secureObject = uie.ToSecureObject();
                secureObjects.Add( secureObject );

                IEnumerable<legacySplxApi.UIElement> children = GetUIElementCollection( uie.ChildObjects );
                if( children != null )
                    RecurseSecureObjectsForImport( children, secureObject.Children );
            }
        }

        static IEnumerable<legacySplxApi.UIElement> GetUIElementCollection(CompositeCollection childObjects)
        {
            foreach( CollectionContainer container in childObjects )
                if( container.Collection is IEnumerable<legacySplxApi.UIElement> )
                    return (IEnumerable<legacySplxApi.UIElement>)container.Collection;

            return null;
        }
    }

    public static class ConverterExtensions
    {
        public static ssp.User ToNewUser(this legacySplxApi.User legacyUser)
        {
            return new ssp.User
            {
                UId = legacyUser.IdToGuid(),
                Name = legacyUser.Name,
                Description = legacyUser.Description,
                IsEnabled = legacyUser.IsEnabled,
                IsLocal = legacyUser.IsLocal,
                IsBuiltIn = legacyUser.IsBuiltIn,
                IsAnonymous = legacyUser.IsAnonymous
            };
        }

        public static ssp.Group ToNewGroup(this legacySplxApi.Group legacyGroup)
        {
            ssp.Group newGroup = new ssp.Group
            {
                UId = legacyGroup.IdToGuid(),
                Name = legacyGroup.Name,
                Description = legacyGroup.Description,
                IsEnabled = legacyGroup.IsEnabled,
                IsLocal = legacyGroup.IsLocal,
                IsBuiltIn = legacyGroup.IsBuiltIn
            };
            if( legacyGroup.Mask != null && legacyGroup.Mask.Length > 0 )
            {
                newGroup.Mask = new byte[legacyGroup.Mask.Length / 8];
                legacyGroup.Mask.CopyTo( newGroup.Mask, 0 );
            }

            return newGroup;
        }

        public static ssp.GroupMembershipItem ToNewGroupMembership(this legacySplxApi.GroupMembershipItem legacyGmi)
        {
            return new ssp.GroupMembershipItem
            {
                GroupUId = legacyGmi.Group.IdToGuid(),
                MemberUId = legacyGmi.Member.IdToGuid(),
                IsMemberUser = legacyGmi.Member.IsUserObject
            };
        }

        public static ssa.SecureObject ToSecureObject(this legacySplxApi.UIElement uie)
        {
            ssa.SecureObject secureObject = new ssa.SecureObject
            {
                UId = uie.Id,
                UniqueName = uie.UniqueName,
                IsEnabled = true,   //prop doesn't exist in legacy objects
                ParentUId = uie.ParentId
            };

            secureObject.Security.DaclAllowInherit = uie.SecurityDescriptor.DaclInherit;
            secureObject.Security.SaclAllowInherit = uie.SecurityDescriptor.SaclInherit;
            secureObject.Security.SaclAuditTypeFilter =
                (ssa.AuditType)Enum.Parse( typeof( ssa.AuditType ), uie.SecurityDescriptor.SaclAuditTypeFilter.ToString() );

            foreach( legacySplxApi.AccessControlEntryBase legacyAce in uie.SecurityDescriptor.Dacl )
                secureObject.Security.Dacl.Add( legacyAce.ToNewAce() );

            foreach( legacySplxApi.AccessControlEntryBase legacyAce in uie.SecurityDescriptor.Sacl )
                secureObject.Security.Sacl.Add( (ssa.IAccessControlEntryAudit)legacyAce.ToNewAce() );

            return secureObject;
        }

        public static ssa.IAccessControlEntry ToNewAce(this legacySplxApi.AccessControlEntryBase legacyAce, bool isAuditAce = false)
        {
            ssa.IAccessControlEntry newAce = null;

            switch( legacyAce.AceType )
            {
                case legacySplxSecurity.AceType.UI:
                {
                    newAce = isAuditAce ? new ssa.AccessControlEntryAudit<ssa.UIRight>() : new ssa.AccessControlEntry<ssa.UIRight>();
                    break;
                }
                case legacySplxSecurity.AceType.Record:
                {
                    newAce = isAuditAce ? new ssa.AccessControlEntryAudit<ssa.RecordRight>() : new ssa.AccessControlEntry<ssa.RecordRight>();
                    break;
                }
                case legacySplxSecurity.AceType.FileSystem:
                {
                    newAce = isAuditAce ? new ssa.AccessControlEntryAudit<ssa.FileSystemRight>() : new ssa.AccessControlEntry<ssa.FileSystemRight>();
                    break;
                }
                case legacySplxSecurity.AceType.Synchronization:
                {
                    newAce = isAuditAce ? new ssa.AccessControlEntryAudit<ssa.SynchronizationRight>() : new ssa.AccessControlEntry<ssa.SynchronizationRight>();
                    break;
                }
            }

            newAce.SetRight( legacyAce.Right.ToString() );
            newAce.Allowed = legacyAce.Allowed;
            if( isAuditAce )
                ((ssa.IAccessControlEntryAudit)newAce).Denied = ((legacySplxSecurity.IAccessControlEntryAudit)legacyAce).Denied;
            newAce.Inheritable = legacyAce.Inherit;
            newAce.TrusteeUId = Guid.Parse( legacyAce.SecurityPrincipalId );
            newAce.UId = Guid.NewGuid();   //incompatible: legacyAce.Id;

            return newAce;
        }
    }
}