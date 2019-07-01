using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QSValidation
{
	public class ObjectValidator : IValidator
	{
		private readonly IValidationViewFactory validationViewFactory;
		private readonly IValidatableObject validatableObject;
		private readonly List<ValidationResult> results;
		private ValidationContext validationContext;

		public bool ShowResultsIfNotValid { get; set; }
		public IEnumerable<ValidationResult> Results => results;

		public ObjectValidator(IValidationViewFactory viewFactory, IValidatableObject validatableObject, ValidationContext validationContext = null)
		{
			ShowResultsIfNotValid = true;
			results = new List<ValidationResult>();
			this.validationViewFactory = viewFactory;
			this.validatableObject = validatableObject;
			this.validationContext = validationContext;
		}

		public bool Validate()
		{
			if(validationContext == null) {
				validationContext = new ValidationContext(validatableObject, null, null);
			}
			return Validate(validationContext);
		}

		public bool Validate(ValidationContext validationContext)
		{
			results.Clear();
			var isValid = Validator.TryValidateObject(validatableObject, validationContext, results, true);
			if(!isValid && ShowResultsIfNotValid) {
				IValidationView view = validationViewFactory.CreateValidationView(results);
				if(view == null) {
					throw new InvalidOperationException("Невозможно создать представление результатов валидации");
				}
				view.ShowModal();
			}
			return isValid;
		}
	}
}
