using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public interface IValidationViewFactory
	{
		IValidationView CreateValidationView(ICollection<ValidationResult> results);
	}
}
