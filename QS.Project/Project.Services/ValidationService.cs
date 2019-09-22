using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

		public IValidator GetValidator(object validatableObject, ValidationContext validationContext = null)
		{
			return new ObjectValidator(validationViewFactory, validatableObject, validationContext);
		}
	}
}
