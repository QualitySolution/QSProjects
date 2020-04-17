using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain
{
	public class ValidatedEntity : IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		public bool FailValidate { get; set; }

		public ValidatedEntity()
		{
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(FailValidate)
				yield return new ValidationResult("Объект не валиден.", new[] { nameof(FailValidate) });
		}
	}
}
