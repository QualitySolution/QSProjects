using System;
using QS.Navigation.TabNavigation.TdiAdapter;
using QS.Navigation.GtkUI;
using Gtk;
using QS.Tdi.Gtk;

namespace QS.GtkUI.Navigation.TabNavigation.TdiAdapter
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TdiTabViewModelAdapterView : Gtk.Bin
	{

		public TdiTabViewModelAdapterView(ITDIWidgetResolver widgetResolver, TdiTabViewModelAdapter tdiAdapter)
		{
			this.Build();
			vboxView.Add(widgetResolver.Resolve(tdiAdapter.Tab));
		}
	}
}
