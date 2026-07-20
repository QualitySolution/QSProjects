using QS.DBScripts.Controllers;
using System;
using System.Collections.Generic;

namespace QS.DbManagement.Creation {
	public class DbResourcesModelMap<TModel> where TModel : class
	{
		private readonly Dictionary<Type, Func<DbCreationResources, TModel>> _map =
			new Dictionary<Type, Func<DbCreationResources, TModel>>();

		public void Register(Type resource, Type creator)
		{
			CheckResourceType(resource);
			if(!typeof(TModel).IsAssignableFrom(creator))
				throw new ArgumentException($"{creator} не реализует {typeof(TModel).Name}", nameof(creator));

			_map[resource] = arg => (TModel)Activator.CreateInstance(creator, arg);
		}

		public void Register(Type resource, Func<DbCreationResources, TModel> creator)
		{
			CheckResourceType(resource);
			_map[resource] = creator ?? throw new ArgumentNullException(nameof(creator));
		}

		public TModel Resolve(DbCreationResources arg)
		{
			if(arg == null)
				throw new ArgumentNullException(nameof(arg));
			if(!_map.TryGetValue(arg.GetType(), out var creator))
				throw new InvalidOperationException($"Нет зарегистрированной модели {typeof(TModel).Name} для ресурса {arg.GetType().Name}");
			return creator(arg);
		}

		private static void CheckResourceType(Type resource)
		{
			if(!typeof(DbCreationResources).IsAssignableFrom(resource))
				throw new ArgumentException($"{resource} не наследует DbCreationResources", nameof(resource));
		}
	}
}
