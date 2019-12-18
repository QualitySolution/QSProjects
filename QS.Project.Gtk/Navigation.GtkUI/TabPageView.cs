using System;
using Gtk;
using QS.Navigation.TabNavigation;

namespace QS.Navigation.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TabPageView : Gtk.Bin
	{
		private readonly TabPageViewModelBase viewModel;
		private readonly IWidgetResolver widgetResolver;

		public TabPageView(TabPageViewModelBase pageViewModel, IWidgetResolver widgetResolver)
		{
			this.Build();
			this.viewModel = pageViewModel ?? throw new ArgumentNullException(nameof(pageViewModel));
			this.widgetResolver = widgetResolver ?? throw new ArgumentNullException(nameof(widgetResolver));

			Widget view = widgetResolver.Resolve(pageViewModel.Page.ViewModel);
			hboxContent.Add(view);
			view.Show();
		}
	}
}
