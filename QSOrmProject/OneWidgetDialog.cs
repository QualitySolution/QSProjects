using System;
using System.ComponentModel;
using Gtk;

namespace QSOrmProject
{
	public partial class OneWidgetDialog : Gtk.Dialog
	{
		public OneWidgetDialog (Widget widget)
		{
			this.Build ();
			widget.Show ();
			VBox.Add (widget);

			var att = widget.GetType ().GetCustomAttributes (typeof(DisplayNameAttribute), true);
			if (att.Length > 0)
				Title = (att [0] as DisplayNameAttribute).DisplayName;
		}
	}
}

