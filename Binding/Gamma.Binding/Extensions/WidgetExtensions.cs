namespace Gamma.Extensions {
	public static class WidgetExtensions {
		public static void DisposeImagePixbuf(this Gtk.Image image)
		{
			image.Pixbuf?.Dispose();
			image.PixbufAnimation?.Dispose();
			image.Pixbuf = null;
			image.PixbufAnimation = null;
		}
	}
}
