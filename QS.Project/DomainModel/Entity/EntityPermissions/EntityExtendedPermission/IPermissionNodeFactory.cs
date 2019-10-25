﻿using System.Collections.Generic;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public interface IPermissionNodeFactory
	{
		IPermissionNode CreateNode();
	}

	public class UserPerrmissionNodeFactory : IPermissionNodeFactory
	{
		public IPermissionNode CreateNode()
		{
			return new UserPermissionNode();
		}
	}
}