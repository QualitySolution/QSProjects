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

		bool useMarkup;
		public bool UseMarkup {
			get {
				return useMarkup;
			}
			set {
				useMarkup = value;
				titleLabel.UseMarkup = value;
			}
		}

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
			set { if (UseMarkup)
					titleLabel.Markup = value;
				else
					titleLabel.Text = value; 
			}
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

		public virtual ButtonMenuAllocation MenuAllocation { get; set; } = ButtonMenuAllocation.Auto;
		public virtual ButtonMenuAlignment MenuAlignment { get; set; } = ButtonMenuAlignment.Auto;

		void Position (Menu menu, out int x, out int y, out bool push_in)
		{
			int gdkX, gdkY;
			GdkWindow.GetOrigin (out gdkX, out gdkY);
			switch(MenuAllocation) {
				case ButtonMenuAllocation.Top:
					y = Allocation.Top + gdkY - menu.Requisition.Height;
					break;
				case ButtonMenuAllocation.Bottom:
					y = Allocation.Bottom + gdkY;
					break;
				case ButtonMenuAllocation.Auto:
				default:
					y = Allocation.Bottom + gdkY;
					if(GdkWindow.Screen.Height < y + menu.Requisition.Height)
						y = Allocation.Top + gdkY - menu.Requisition.Height;
					break;
			}

			switch(MenuAlignment) {
				case ButtonMenuAlignment.Left:
					x = Allocation.X + gdkX;
					break;
				case ButtonMenuAlignment.Right:
					x = Allocation.X + gdkX + Allocation.Width - menu.Requisition.Width;
					break;
				case ButtonMenuAlignment.Auto:
				default:
					x = Allocation.X + gdkX;
					if(Allocation.Width > menu.Requisition.Width)
						menu.WidthRequest = Allocation.Width;
					break;
			}
			push_in = true;
		}
	}

	public enum ButtonMenuAllocation
	{
		Auto,
		Top,
		Bottom
	}

	public enum ButtonMenuAlignment
	{
		Auto,
		Left,
		Right
	}
}

