using System;
namespace QS.Services
{
	public interface IPermissionService
	{
		IPermissionResult ValidateEntityPermissionForUser(Type entityType, int userId);
		bool ValidatePresetPermissionForUser(string permissionName, int userId);

		IPermissionResult ValidateEntityPermissionForCurrentUser(Type entityType);
		bool ValidatePresetPermissionForCurrentUser(string permissionName);
	}

	public interface IPermissionResult
	{
		bool CanCreate 	{ get; }
		bool CanRead 	{ get; }
		bool CanUpdate 	{ get; }
		bool CanDelete 	{ get; }
	}
}
