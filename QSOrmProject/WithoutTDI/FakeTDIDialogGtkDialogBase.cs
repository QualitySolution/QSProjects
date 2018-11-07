using System;
using QS.Tdi;

namespace QSOrmProject
{
	public class FakeTDIDialogGtkDialogBase : FakeTDITabGtkDialogBase, ITdiDialog
	{
		public FakeTDIDialogGtkDialogBase ()
		{
		}

		public bool Save ()
		{
			throw new NotImplementedException ();
		}

		public void SaveAndClose()
		{
			throw new NotImplementedException();
		}

		public bool HasChanges {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

