using System;
using System.Collections.Generic;
using Gdk;
using Gtk;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class VImagesList : Gtk.Bin
	{
		public List<Pixbuf> Images;

		public VImagesList ()
		{
			this.Build ();
			Images = new List<Pixbuf> ();
		}

		public void UpdateList()
		{
			foreach(Widget wid in vboxImages.Children )
			{
				wid.Destroy ();
				vboxImages.Remove (wid);
			}

			foreach(Pixbuf pix in Images)
			{
				ImageViewer view = new ImageViewer();
				view.VerticalFit = false;
				view.HorizontalFit = true;
				view.Pixbuf = pix;
				//view.ButtonPressEvent += OnImagesButtonPressEvent;
				vboxImages.Add(view);
			}
			vboxImages.ShowAll ();
		}
	}
}

