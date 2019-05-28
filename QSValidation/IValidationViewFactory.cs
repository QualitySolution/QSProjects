using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QSValidation
{
	public interface IValidationViewFactory
	{
		IValidationView CreateValidationView(List<ValidationResult> results);
	}
}
