using System;
using QS.Dialog.GtkUI;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Views.GtkUI
{
	public abstract class TabViewBase : Gtk.Bin, ITabView
	{
		public virtual ITdiTab Tab { get; }
	}
}
