using System;
namespace QS.Services
{
	public interface ICommonServices
	{
		IValidationService ValidationService { get; }
		IInteractiveService InteractiveService { get; }
		IPermissionService PermissionService { get; }
		ICurrentPermissionService CurrentPermissionService { get; }
		IUserService UserService { get; }
	}
}