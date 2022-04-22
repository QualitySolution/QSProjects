using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public interface IValidator
	{
		/// <param name="validatableObject">Валидируемый объект</param>
		/// <param name="validationContext">Контекст валидации</param>
		/// <param name="showValidationResults">Указывает, нужно ли показывать сообщение с результатами валидации (Только если объект не прошёл валидацию)</param>
		/// <returns>Возвращает <see langword="true"/> если объект корректен.</returns>
		bool Validate(object validatableObject, ValidationContext validationContext = null, bool showValidationResults = true);

		/// <param name="requests">Список валидируемых объектов и контекстов</param>
		/// <param name="showValidationResults">Указывает, нужно ли показывать сообщение с результатами валидации (Только если объект не прошёл валидацию)</param>
		/// <returns>Возвращает <see langword="true"/> если объекты корректные.</returns>
		bool Validate(IEnumerable<ValidationRequest> requests, bool showValidationResults = true);

		/// <summary>
		/// Детальные результаты последней проверки.
		/// </summary>
		IEnumerable<ValidationResult> Results { get; }
	}
}
