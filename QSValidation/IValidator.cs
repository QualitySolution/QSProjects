using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QSValidation
{
	public interface IValidator
	{
		bool ShowResultsIfNotValid { get; set; }
		bool Validate();
		bool Validate(IDictionary<object, object> contextItems);
		IEnumerable<ValidationResult> Results { get; }
	}
}
