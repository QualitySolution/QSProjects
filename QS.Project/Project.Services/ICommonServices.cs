using System;
using QS.Dialog;
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