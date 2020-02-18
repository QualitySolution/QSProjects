using System;
using QS.Validation;

namespace QS.Services
{
	public interface ICommonServices
	{
		IValidator ValidationService { get; }
		IInteractiveService InteractiveService { get; }
		IPermissionService PermissionService { get; }
		ICurrentPermissionService CurrentPermissionService { get; }
		IUserService UserService { get; }
	}
}