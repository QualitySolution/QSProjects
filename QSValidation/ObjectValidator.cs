using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QSValidation
{
	public class ObjectValidator : IValidator
	{
		private readonly IValidationViewFactory validationViewFactory;
		private readonly IValidatableObject validatableObject;
		private readonly IDictionary<object, object> contextItems;
		private readonly List<ValidationResult> results;

		public bool ShowResultsIfNotValid { get; set; }
		public IEnumerable<ValidationResult> Results => results;

		public ObjectValidator(IValidationViewFactory viewFactory, IValidatableObject validatableObject, IDictionary<object, object> contextItems = null)
		{
			ShowResultsIfNotValid = true;
			results = new List<ValidationResult>();
			this.validationViewFactory = viewFactory;
			this.validatableObject = validatableObject;
			this.contextItems = contextItems;
		}

		public bool Validate()
		{
			return Validate(contextItems);
		}

		public bool Validate(IDictionary<object, object> contextItems)
		{
			var vc = new ValidationContext(validatableObject, null, contextItems);
			results.Clear();
			var isValid = Validator.TryValidateObject(validatableObject, vc, results, true);
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
