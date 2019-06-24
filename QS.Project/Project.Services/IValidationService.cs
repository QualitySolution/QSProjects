using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSValidation;

namespace QS.Services
{
	public interface IValidationService
	{
		IValidator GetValidator(IValidatableObject validatableObject, ValidationContext validationContext = null);
	}
}