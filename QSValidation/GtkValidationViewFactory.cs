using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Validation;

namespace QSValidation
{
	public class GtkValidationViewFactory : IValidationViewFactory
	{
		public IValidationView CreateValidationView(List<ValidationResult> results)
		{
			return new ResultsListDlg(results);
		}
	}
}
