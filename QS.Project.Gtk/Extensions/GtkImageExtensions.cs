namespace QS.Extensions
{
	public static class GtkImageExtensions
	{
		public static void DisposeImagePixbuf(this Gtk.Image image)
		{
			if(image.Pixbuf == null)
			{
				return;
			}

			image.Pixbuf.Dispose();
			image.Pixbuf = null;
		}
	}
}
