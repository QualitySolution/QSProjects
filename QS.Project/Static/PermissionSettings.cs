using System;
using System.Collections.Generic;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
namespace QS.Static
{
	public static class PermissionsMain
	{
		public static IEntityPermissionValidator EntityPermissionValidator { get; set; }

		static PermissionsMain()
		{
			EntityPermissionValidator = null;
		}

		public static string GetEntityReadValidateResult(Type entityType)
		{
			var aa = entityType.GetCustomAttribute<AppellativeAttribute>();
			string message;
			if(aa == null || String.IsNullOrWhiteSpace(aa.Nominative)) {
				message = $"У вас нет прав для просмотра документов типа: {entityType.Name}";
			} else {
				message = $"У Вас нет прав для просмотра следующих типов документов: {aa.Nominative}";
			}
			return message;
		}


		/// <summary>
		/// Однократное нахождение всех типов помеченных аттрибутом <see cref="EntityPermissionAttribute"/>
		/// </summary>
		/// <param name="entitiesFinder">объект который ищет необходиые типы с настройками под конкретный проект</param>
		public static void ConfigureEntityPermissionFinder(IEntitiesWithPermissionFinder entitiesFinder)
		{
			permissionsEntityTypes = entitiesFinder.FindTypes();
		}

		private static IEnumerable<Type> permissionsEntityTypes;

		/// <summary>
		/// Список типов помеченных аттрибутом <see cref="EntityPermissionAttribute"/>
		/// </summary>
		/// <value>The permissions entity types.</value>
		public static IEnumerable<Type> PermissionsEntityTypes {
			get{
				if(permissionsEntityTypes == null) {
					return new List<Type>();
				}
				return permissionsEntityTypes;
			}
		}
	}
}
