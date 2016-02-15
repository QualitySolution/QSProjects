using System;
using QSProjectsLib;

namespace QSOrmProject.Deletion
{
	public partial class DeleteOperationDlg : Gtk.Window
	{
		public DeleteOperationDlg()
			: base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}

		public void SetOperationsCount(int count)
		{
			progressbarOperation.Adjustment.Upper = count;
		}

		public void AddExcuteOperation(string text)
		{
			labelOperation.LabelProp = text;
			progressbarOperation.Adjustment.Value++;
			QSMain.WaitRedraw();
		}
	}
}

