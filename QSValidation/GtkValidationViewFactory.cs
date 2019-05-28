using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
