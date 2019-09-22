using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public class ObjectValidator : IValidator
	{
		private readonly IValidationViewFactory validationViewFactory;
		private readonly List<ValidationResult> results = new List<ValidationResult>();

		public IEnumerable<ValidationResult> Results => results;

		/// <summary>
		/// Валидатор с указанной фабрикой вюъшек отображает диалог с ошибкой через фабрику.
		/// </summary>
		public ObjectValidator(IValidationViewFactory viewFactory) 
		{
			this.validationViewFactory = viewFactory;
		}

		/// <summary>
		/// Валидатор без фабрики вьюшек просто проверяет объект.
		/// </summary>
		public ObjectValidator()
		{
		}

		public bool Validate(object validatableObject, ValidationContext validationContext = null)
		{
			results.Clear();

			if(validationContext == null) {
				validationContext = new ValidationContext(validatableObject, null, null);
			}

			var isValid = Validator.TryValidateObject(validatableObject, validationContext, results, true);
			if(!isValid && validationViewFactory != null) {
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
