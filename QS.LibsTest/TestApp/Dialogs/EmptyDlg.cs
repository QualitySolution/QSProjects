using System;
using QS.Dialog.Gtk;
using QS.Tdi;

namespace QS.Test.TestApp.Dialogs
{
	public partial class EmptyDlg : TdiTabBase, ITdiDialog
	{
		public EmptyDlg()
		{
			this.Build();
		}

		public bool HasChanges => false;

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool Save()
		{
			throw new NotImplementedException();
		}

		public void SaveAndClose()
		{
			throw new NotImplementedException();
		}
	}
}
