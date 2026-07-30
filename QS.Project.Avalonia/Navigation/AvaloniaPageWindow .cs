using System;
using Avalonia.Controls;
using QS.ViewModels.Extension;

namespace QS.Navigation;

// окно для страницы-диалога
internal class AvaloniaPageWindow : Window {
	readonly IAvaloniaWindowPage page;
	readonly Func<IPage, bool> canClose;
	readonly Action<IPage> close;
	readonly bool deletable;

	public AvaloniaPageWindow(IAvaloniaWindowPage page, Func<IPage, bool> canClose, Action<IPage> close) {
		this.page = page;
		this.canClose = canClose;
		this.close = close;

		var settings = page.ViewModel as IWindowDialogSettings;
		deletable = settings?.Deletable ?? true;

		Title = page.ViewModel.Title;
		Content = page.View;
		SizeToContent = SizeToContent.WidthAndHeight;
		CanResize = settings?.Resizable ?? true;
		ShowInTaskbar = settings?.EnableMinimizeMaximize ?? false;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;
		page.ViewModel.PropertyChanged += (s, e) => Title = page.ViewModel.Title;
	}

	protected override void OnClosing(WindowClosingEventArgs e) {
		base.OnClosing(e);
		if(page.Window == null)
			return; // проверки уже сделаны

		if(!deletable || !canClose(page))
			e.Cancel = true;
	}

	protected override void OnClosed(EventArgs e) {
		base.OnClosed(e);
		if(page.Window == null)
			return;

		page.Window = null;
		close(page);
	}
}
