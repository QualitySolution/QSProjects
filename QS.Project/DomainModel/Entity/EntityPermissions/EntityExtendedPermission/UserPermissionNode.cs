using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public class UserPermissionNode : IPermissionNode
	{
		public TypeOfEntity TypeOfEntity { get; set; }
		public EntityUserPermission EntityUserOnlyPermission { get; set; }
		public IList<EntityUserPermissionExtended> EntityPermissionExtended { get; set; }

		public EntityPermissionBase EntityPermission => EntityUserOnlyPermission;

		IList<EntityPermissionExtendedBase> IPermissionNode.EntityPermissionExtended {
			get => EntityPermissionExtended.OfType<EntityPermissionExtendedBase>().ToList();
			set => EntityPermissionExtended = value.OfType<EntityUserPermissionExtended>().ToList();
		}
	}
}
