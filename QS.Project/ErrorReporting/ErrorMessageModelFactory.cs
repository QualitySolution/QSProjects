using System;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.ErrorReporting
{
	public class ErrorMessageModelFactory : IErrorMessageModelFactory
	{
		IErrorReporter errorReporter;
		IUserService userService;
		IUnitOfWorkFactory unitOfWorkFactory;

		public ErrorMessageModelFactory(
			IErrorReporter errorReporter,
			IUserService userService,
			IUnitOfWorkFactory unitOfWorkFactory
			)
		{
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			this.userService = userService ?? throw new ArgumentNullException(nameof(errorReporter));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(errorReporter));
		}

		public ErrorMessageModelBase GetModel()
		{
			return new ErrorMessageModel(
				errorReporter,
				userService,
				unitOfWorkFactory
			);
		}
	}
}
