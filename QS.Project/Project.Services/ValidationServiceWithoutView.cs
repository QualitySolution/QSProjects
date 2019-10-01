using System;
using QS.Services;
using QS.Validation;

namespace QS.Project.Services
{
	public class ValidationServiceWithoutView : IValidationService
	{
		public IValidator GetValidator()
		{
			return new ObjectValidator();
		}
	}
}
