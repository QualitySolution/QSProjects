using System;
using QS.Project.Domain;
namespace QS.Services
{
	public interface IPermissionService
	{
		IPermissionResult ValidateUserPermission(Type entityType, int userId);
	}

	public interface IPermissionResult
	{
		bool CanCreate 	{ get; }
		bool CanRead 	{ get; }
		bool CanUpdate 	{ get; }
		bool CanDelete 	{ get; }
	}
}
