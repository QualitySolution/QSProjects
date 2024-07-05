using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public class ObjectValidator : IValidator
	{
		private readonly IValidationViewFactory validationViewFactory;
		private readonly HashSet<ValidationResult> results = new HashSet<ValidationResult>();

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

		/// <returns>Возвращает <see langword="true"/> если объект корректен.</returns>
		public bool Validate(object validatableObject, ValidationContext validationContext = null, bool showValidationResults = true)
		{
			if(validationContext is null) {
				validationContext = new ValidationContext(validatableObject);
			}

			if(ServiceProvider != null) {
				validationContext.InitializeServiceProvider((type) => ServiceProvider.GetRequiredService(type));
			}

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

			if(!isValid && showValidationResults && validationViewFactory != null) {
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
