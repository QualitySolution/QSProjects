using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;

namespace QS.Validation.Testing
{
	public class ValidatorForTests : IValidator
	{
		private readonly List<ValidationResult> results = new List<ValidationResult>();

		public IEnumerable<ValidationResult> Results => results;

		/// <returns>Возвращает <see langword="true"/> если объект корректен.</returns>
		public bool Validate(object validatableObject, ValidationContext validationContext = null, bool showValidationResults = true)
		{
			return Validate(new[] { new ValidationRequest(validatableObject, validationContext)}, showValidationResults);
		}

		/// <returns>Возвращает <see langword="true"/> если объекты корректны.</returns>
		public bool Validate(IEnumerable<ValidationRequest> requests, bool showValidationResults = true)
		{
			results.Clear();

			var isValid = true;

			foreach(var request in requests) {
				var isItemValid = Validator.TryValidateObject(request.ValidateObject, request.ValidationContext, results, true);
				isValid &= isItemValid;
			}

			if(!isValid && showValidationResults) {
				Assert.Fail("Валидация не прошла:\n" +
				            String.Join("\n", results.Select(x => "* " + x.ErrorMessage)));
			}
			return isValid;
		}
	}
}
