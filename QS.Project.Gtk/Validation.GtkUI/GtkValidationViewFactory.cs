using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Validation;

namespace QS.Validation.GtkUI
{
	public class GtkValidationViewFactory : IValidationViewFactory
	{
		public IValidationView CreateValidationView(List<ValidationResult> results)
		{
			return new ResultsListDlg(results);
		}
	}
}
