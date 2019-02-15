using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Domain;

namespace QS.Project.Repositories
{
	public static class TypeOfEntityRepository
	{
		public static string GetRealName(Type type)
		{
			var result = type?.GetCustomAttributes(typeof(AppellativeAttribute), false)
				.Cast<AppellativeAttribute>()
				.FirstOrDefault()
				.Nominative;

			return result;
		}

		public static Type GetEntityType(string strType)
		{
			var items = PermissionsSettings.PermissionsEntityTypes;
			return items.FirstOrDefault(t => t.Name == strType);
		}

		public static string GetEntityNameByString(string strType) => GetRealName(GetEntityType(strType));

		public static IList<Type> GetEntityTypesMarkedByEntityPermissionAttribute(bool hideExistingInPermissions = false)
		{
			var result = PermissionsSettings.PermissionsEntityTypes;
			if(hideExistingInPermissions)
				return result.Where(t => !GetAllTypesOfEntity().Contains(t.Name)).ToList();
			return result.ToList();
		}

		public static IList<string> GetAllTypesOfEntity()
		{
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				return uow.Session.QueryOver<TypeOfEntity>().List().Select(t => t.Type).ToList();
			}
		}

		public static IList<TypeOfEntity> GetAllSavedTypeOfEntity(IUnitOfWork uow)
		{
			return uow.GetAll<TypeOfEntity>().ToList();
		}
	}
}
