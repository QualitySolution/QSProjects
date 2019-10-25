﻿using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using System.Collections.ObjectModel;
using QS.EntityRepositories;
using System;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using System.Linq;

namespace QS.Project.Repositories
{
	[Obsolete("Используйте одноимённый класс из QS.EntityRepositories.UserPermissionSingletonRepository")]
	public static class UserPermissionRepository
	{
		public static EntityUserPermission GetUserEntityPermission(IUnitOfWork uow, string entityName, int userId)
		{
			return UserPermissionSingletonRepository.GetInstance().GetUserEntityPermission(uow, entityName, userId);
		}

		public static IList<UserPermissionNode> GetUserAllEntityPermissions(IUnitOfWork uow, int userId, IPermissionExtensionStore permissionExtensionStore)
		{
			return UserPermissionSingletonRepository.GetInstance().GetUserAllEntityPermissions(uow, userId, permissionExtensionStore).ToList();
		}

		public static IList<PresetUserPermission> GetUserAllPresetPermissions(IUnitOfWork uow, int userId)
		{
			return UserPermissionSingletonRepository.GetInstance().GetUserAllPresetPermissions(uow, userId);
		}

		public static IReadOnlyDictionary<string, bool> CurrentUserPresetPermissions { 
			get { return UserPermissionSingletonRepository.GetInstance().CurrentUserPresetPermissions; }
		}
	}
}
