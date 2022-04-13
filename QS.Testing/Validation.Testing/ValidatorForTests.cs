using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;

namespace QS.Validation.Testing
{
	public class ValidatorForTests : IValidator
	{
		private readonly bool ignoreInvalid;
		private readonly List<ValidationResult> results = new List<ValidationResult>();

		public ValidatorForTests(bool ignoreInvalid = false)
		{
			this.ignoreInvalid = ignoreInvalid;
		}

		public IEnumerable<ValidationResult> Results => results;

		/// <returns>Возвращает <see langword="true"/> если объект корректен.</returns>
		public bool Validate(object validatableObject, ValidationContext validationContext = null)
		{
			return Validate(new[] { new ValidationRequest(validatableObject, validationContext)});
		}

		/// <returns>Возвращает <see langword="true"/> если объекты корректны.</returns>
		public bool Validate(IEnumerable<ValidationRequest> requests)
		{
			results.Clear();

			var isValid = true;

			foreach(var request in requests) {
				var isItemValid = Validator.TryValidateObject(request.ValidateObject, request.ValidationContext, results, true);
				isValid &= isItemValid;
			}

			if(!isValid && !ignoreInvalid) {
				Assert.Fail("Валидация не прошла:\n" +
				            String.Join("\n", results.Select(x => "* " + x.ErrorMessage)));
			}
			return isValid;
		}
	}
}
