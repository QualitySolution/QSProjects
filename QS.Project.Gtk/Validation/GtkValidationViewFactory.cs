using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.Validation
{
	public class GtkValidationViewFactory : IValidationViewFactory
	{
		public IValidationView CreateValidationView(ICollection<ValidationResult> results)
		{
			return new ResultsListDlg(results);
		}
	}
}
