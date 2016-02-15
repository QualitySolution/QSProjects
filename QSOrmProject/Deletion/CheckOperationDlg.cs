using System;
using QSProjectsLib;

namespace QSOrmProject
{
	public partial class CheckOperationDlg : Gtk.Window
	{
		int linksCount;

		public bool IsCanceled = false;

		public CheckOperationDlg()
			: base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}

		public void SetOperationName(string text)
		{
			labelOperation.LabelProp = text;
			QSMain.WaitRedraw();
		}

		public void AddLinksCount(int count)
		{
			linksCount += count;
			labelDependence.LabelProp = String.Format("Всего найдено ссылок: {0}", linksCount);
			QSMain.WaitRedraw();
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			IsCanceled = true;
		}

		protected void OnDeleteEvent (object sender, EventArgs e)
		{
			IsCanceled = true;
		}
	}
}

