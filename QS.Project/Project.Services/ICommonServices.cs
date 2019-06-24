using System;
namespace QS.Services
{
	public interface ICommonServices
	{
		IValidationService ValidationService { get; }
		IInteractiveService InteractiveService { get; }
		IPermissionService PermissionService { get; }
		IUserService UserService { get; }
	}
}