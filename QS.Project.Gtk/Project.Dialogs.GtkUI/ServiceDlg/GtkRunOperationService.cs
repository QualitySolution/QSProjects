using System;
using QS.Project.Services;

namespace QS.Project.Dialogs.GtkUI.ServiceDlg
{
	public class GtkRunOperationService : IRunOperationService
	{
		public IRunOperationView GetRunOperationView()
		{
			return new RunOperationView();
		}
	}
}
