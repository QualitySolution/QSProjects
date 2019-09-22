using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Validation;

namespace QS.Validation.GtkUI
{
	public partial class ResultsListDlg : Gtk.Dialog, IValidationView
	{
		public ResultsListDlg (List<ValidationResult> list )
		{
			this.Build ();

			foreach(ValidationResult valid in list)
			{
				var ms = new ResultItem (valid.ErrorMessage);
				vboxMessages.PackStart (ms, false, false, 0);
			}
			vboxMessages.ShowAll ();
		}

		public void ShowModal()
		{
			Modal = true;
			Show();
			Run();
			Destroy();
		}
	}
}

