using System;
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
	}
}
