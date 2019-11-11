using System;
using QS.Dialog.Gtk;
using QS.Tdi;

namespace QS.Test.TestApp.Dialogs
{
	public partial class EmptyJournalTab : TdiTabBase, ITdiJournal
	{
		public EmptyJournalTab()
		{
			this.Build();
		}

		public bool? UseSlider { get; set; }
	}
}
