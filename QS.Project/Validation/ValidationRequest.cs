using System;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public class ValidationRequest
	{
		public ValidationRequest(object validateObject, ValidationContext context = null)
		{
			ValidateObject = validateObject;

			if(context == null) 
				ValidationContext = new ValidationContext(validateObject, null, null);
			else 
				ValidationContext = context;
		}

		public object ValidateObject { get; }
		public ValidationContext ValidationContext { get; set; }
	}
}
