using QS.Tdi;

namespace QS.Views.GtkUI
{
	public abstract class TabViewBase : Gtk.Bin, ITabView
	{
		public virtual ITdiTab Tab { get; }
	}
}
