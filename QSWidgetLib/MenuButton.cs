using System;
using System.ComponentModel;
using Gtk;

namespace QSWidgetLib
{
	[ToolboxItem (true)]
	public class MenuButton : Button
	{
		Menu popup_menu;

		public Menu Menu {
			get {
				return popup_menu;
			}
			set {
				popup_menu = value;
			}
		}

		public MenuButton ()
		{
			var arrow = new Arrow (ArrowType.Down, ShadowType.None);
			arrow.Show ();
			this.Add (arrow);
		}

		protected override void OnPressed ()
		{
			if (popup_menu == null)
				return;

			popup_menu.Popup (null, null, Position, 0, Gtk.Global.CurrentEventTime);
		}

		void Position (Menu menu, out int x, out int y, out bool push_in)
		{
			int gdkX, gdkY;
			GdkWindow.GetOrigin (out gdkX, out gdkY);

			x = this.Allocation.X + gdkX;
			y = this.Allocation.Bottom + gdkY;
			if (GdkWindow.Screen.Height < y + menu.Requisition.Height)
				y = this.Allocation.Top + gdkY - menu.Requisition.Height;
			push_in = true;

			if (Allocation.Width > menu.Requisition.Width)
				menu.WidthRequest = Allocation.Width;
		}
	}
}

