using System;
using System.ComponentModel;
using Gtk;

namespace QSWidgetLib
{
	[ToolboxItem (true)]
	public class MenuButton : Button
	{
		Label titleLabel;
		HBox hbox;
		Arrow arrow;
		Menu popup_menu;

		public Menu Menu {
			get {
				return popup_menu;
			}
			set {
				popup_menu = value;
			}
		}

		[Browsable(false)]
		public new string Label {
			get { return titleLabel.Text; }
			set { titleLabel.Text = value; }
		}

		Image image;
		public new Image Image {
			get {
				return image;
			}
			set {
				if (image == value)
					return;
				hbox.Remove (image);
				image = value;

				hbox.PackStart (Image, false, false, 1);
				hbox.ReorderChild (Image, 0);
			}
		}

		public MenuButton ()
		{
			hbox = new HBox ();

			Image = new Image ();
			hbox.PackStart (Image, false, false, 1);
			Image.Show ();

			this.titleLabel = new Label ();
			this.titleLabel.Xalign = 0;
			hbox.PackStart (this.titleLabel, true, true, 1);
			this.titleLabel.Show ();

			this.arrow = new Arrow (ArrowType.Down, ShadowType.None);
			hbox.PackStart (arrow, false, false, 1);
			arrow.Show ();

			this.Add (hbox);
			hbox.Show ();
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

