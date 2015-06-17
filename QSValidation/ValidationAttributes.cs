using System;
using System.ComponentModel.DataAnnotations;

namespace QSValidation.Attributes
{
	public class DateRequiredAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			if (value is DateTime)
				return (DateTime)value != default(DateTime);
			else
				return false;
		}
	}
}

