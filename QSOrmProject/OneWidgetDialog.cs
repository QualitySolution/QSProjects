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

			var att = widget.GetType ().GetCustomAttributes (typeof(WidgetWindowAttribute), false);
			if (att.Length > 0)
				this.SetDefaultSize ((att [0] as WidgetWindowAttribute).DefaultWidth,
				                     (att [0] as WidgetWindowAttribute).DefaultHeight);

			widget.Show ();
			VBox.Add (widget);

			att = widget.GetType ().GetCustomAttributes (typeof(DisplayNameAttribute), true);
			if (att.Length > 0)
				Title = (att [0] as DisplayNameAttribute).DisplayName;
			this.ReshowWithInitialSize ();
		}
	}
}

