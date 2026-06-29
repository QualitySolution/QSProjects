using QS.DBScripts.Controllers;
using System;
using System.Collections.Generic;

namespace QS.DbManagement.Creation {
	public class DbResourcesCreationMap
	{
		private Dictionary<Type, Func<DbCreationResources, object>> _map = new Dictionary<Type, Func<DbCreationResources, object>>();

		public void Register(Type resource, Type creator)
		{
			if(!typeof(DbCreationResources).IsAssignableFrom(resource))
				throw new ArgumentException($"{resource} не наследует DbCreationResources", nameof(resource));

			if(!typeof(IDbCreatorModel).IsAssignableFrom(creator))
				throw new ArgumentException($"{creator} не реализует IDbCreatorModel", nameof(creator));

			_map[resource] = arg => Activator.CreateInstance(creator, arg);
		}

		public object Resolve(DbCreationResources arg) => _map[arg.GetType()](arg);
	}
}
