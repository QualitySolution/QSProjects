using System;

namespace QS.Validation.GtkUI
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ResultItem : Gtk.Bin
	{
		public ResultItem ()
		{
			this.Build ();
		}

		public ResultItem (string message) : this()
		{
			labelMessage.LabelProp = message;
		}

	}
}

