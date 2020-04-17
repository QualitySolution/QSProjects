using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public interface IValidator
	{
		/// <returns>Возвращает <see langword="true"/> если объект корректен.</returns>
		bool Validate(object validatableObject, ValidationContext validationContext = null);
		/// <summary>
		/// Детальные результаты последней проверки.
		/// </summary>
		IEnumerable<ValidationResult> Results { get; }
	}
}
