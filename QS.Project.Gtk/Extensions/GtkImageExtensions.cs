namespace QS.Extensions
{
	public static class GtkImageExtensions
	{
		public static void DisposeImagePixbuf(this Gtk.Image image)
		{
			image.Pixbuf?.Dispose();
			image.PixbufAnimation?.Dispose();
			image.Pixbuf = null;
			image.PixbufAnimation = null;
		}
	}
}
