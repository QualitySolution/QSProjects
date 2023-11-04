using Gtk;

namespace QS.Extensions {
	public static class HboxExtensions {
		public static void PackFromStart(
			this HBox hbox,
			Widget widget,
			bool expand = false,
			bool fill = false,
			uint padding = 0) {
			hbox.PackStart(widget, expand, fill, padding);
		}
		
		public static void PackFromEnd(
			this HBox hbox,
			Widget widget,
			bool expand = false,
			bool fill = false,
			uint padding = 0) {
			hbox.PackEnd(widget, expand, fill, padding);
		}
	}
}
