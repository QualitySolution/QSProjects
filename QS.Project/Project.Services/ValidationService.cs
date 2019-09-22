using QS.Validation;

namespace QS.Services
{
	public class ValidationService : IValidationService
	{
		private readonly IValidationViewFactory validationViewFactory;

		public ValidationService(IValidationViewFactory validationViewFactory)
		{
			this.validationViewFactory = validationViewFactory;
		}

		public IValidator GetValidator()
		{
			return new ObjectValidator(validationViewFactory);
		}
	}
}
