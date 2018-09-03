using System;
using System.Collections.Generic;
using Gdk;
using Gtk;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class VImagesList : Gtk.Bin
	{
		public List<Pixbuf> Images;
		private Dictionary<Pixbuf, object> imagesTags = new Dictionary<Pixbuf, object>();

		public event EventHandler<ImageButtonPressEventArgs> ImageButtonPressEvent;

		public VImagesList()
		{
			this.Build();
			Images = new List<Pixbuf>();
		}

		public void AddImage(Pixbuf pixbuf, object tag = null)
		{
			Images.Add(pixbuf);
			if(tag != null)
				imagesTags.Add(pixbuf, tag);
		}

		public void UpdateList()
		{
			foreach(Widget wid in vboxImages.Children) {
				wid.Destroy();
				vboxImages.Remove(wid);
			}

			foreach(Pixbuf pix in Images) {
				ImageViewer view = new ImageViewer();
				view.VerticalFit = false;
				view.HorizontalFit = true;
				view.Pixbuf = pix;
				view.ButtonPressEvent += OnImagesButtonPressEvent;
				vboxImages.Add(view);
			}
			vboxImages.ShowAll();
		}

		private void OnImagesButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			var imgWid = (ImageViewer)o;
			object tag = null;
			if(imagesTags.ContainsKey(imgWid.Pixbuf))
				tag = imagesTags[imgWid.Pixbuf];

			ImageButtonPressEvent?.Invoke(o,
										  new ImageButtonPressEventArgs {
											  eventArgs = args,
											  Tag = tag
										  });
		}
	}

	public class ImageButtonPressEventArgs : System.EventArgs
	{
		public ButtonPressEventArgs eventArgs;
		public object Tag;
	}
}

