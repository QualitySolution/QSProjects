using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public class PermissionExtensionSingletonStore : IPermissionExtensionStore
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static PermissionExtensionSingletonStore instance;

		public static IEnumerable<string> AssembliesFilter { get; set; }

		public static PermissionExtensionSingletonStore GetInstance(IEnumerable<string> assembliesFilter = null) {
			AssembliesFilter = assembliesFilter;
			if(instance == null)
				instance = new PermissionExtensionSingletonStore();
			return instance;
		}

		protected PermissionExtensionSingletonStore() { }

		private IList<IPermissionExtension> permissionExtensions;
		public IList<IPermissionExtension> PermissionExtensions {
			get 
			{
				if(permissionExtensions == null)
					permissionExtensions = GetExtensions();

				return permissionExtensions;
			}
		}

		protected IList<IPermissionExtension> GetExtensions()
		{
			IList<IPermissionExtension> extensions = new List<IPermissionExtension>();
			Type parent = typeof(IPermissionExtension);
			IEnumerable<Type> types = new List<Type>();

			var currenDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assemblies = AssembliesFilter == null
				? currenDomainAssemblies
				: currenDomainAssemblies.Where(c => AssembliesFilter.Any(f => c.FullName.StartsWith(f)));

			foreach(var assembly in assemblies) {
				var list = assembly.GetTypes().Where(x => parent.IsAssignableFrom(x) && !x.IsAbstract);
				if(list?.FirstOrDefault() != null)
					types = types.Concat(list);
			}
			foreach(var item in types) {
				try 
				{
					if(Activator.CreateInstance(item) is IPermissionExtension instance)
						extensions.Add(instance);
				}
				catch(MissingMethodException ex) {
					logger.Error(ex, $"Ошибка при создании экземпляра класса {item.Name}, у класса отсутствует пустой конструктор");
					continue;
				}
			}

			return extensions;
		}


	}
}
