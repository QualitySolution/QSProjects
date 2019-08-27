using System;
namespace QS.Services
{
	public interface IPermissionService
	{
		IPermissionResult ValidateUserPermission(Type entityType, int userId);
		bool ValidateUserPresetPermission(string permissionName, int userId);
	}

	public interface IPermissionResult
	{
		bool CanCreate 	{ get; }
		bool CanRead 	{ get; }
		bool CanUpdate 	{ get; }
		bool CanDelete 	{ get; }
	}
}
