using System;
using System.Collections.Generic;
using System.Reflection;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions
{
	public interface IEntitiesWithPermissionFinder
	{
		IEnumerable<Type> FindTypes();
	}

	public class EntitiesWithPermissionFinder : IEntitiesWithPermissionFinder
	{
		public IEnumerable<Type> FindTypes()
		{
			var qsDomainAssembly = Assembly.GetAssembly(typeof(EntityUserPermission));
			return DomainHelper.GetHavingAttributeEntityTypes<EntityPermissionAttribute>(x => x.IsClass && !x.IsAbstract, qsDomainAssembly);
		}
	}
}
