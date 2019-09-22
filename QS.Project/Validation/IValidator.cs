using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public interface IValidator
	{
		bool Validate(object validatableObject, ValidationContext validationContext = null);
		IEnumerable<ValidationResult> Results { get; }
	}
}
