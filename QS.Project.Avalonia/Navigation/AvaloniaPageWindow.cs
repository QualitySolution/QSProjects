using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using QS.ViewModels.Extension;

namespace QS.Navigation;

// окно для страницы-диалога
internal class AvaloniaPageWindow : Window {
	readonly IAvaloniaWindowPage page;
	readonly Func<IPage, bool> canClose;
	readonly Action<IPage> close;
	readonly bool deletable;
	readonly PropertyChangedEventHandler titleChanged;

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
		var minimizeMaximize = settings?.EnableMinimizeMaximize ?? false;
		CanMinimize = minimizeMaximize;
		CanMaximize = minimizeMaximize;
		ShowInTaskbar = minimizeMaximize;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;

		var screen = Screens.Primary;
		if(screen != null) {
			MaxWidth = screen.WorkingArea.Width / screen.Scaling;
			MaxHeight = screen.WorkingArea.Height / screen.Scaling;
		}

		titleChanged = (_, e) => {
			if(e.PropertyName == nameof(IDialogViewModel.Title))
				Title = page.ViewModel.Title;
		};
		page.ViewModel.PropertyChanged += titleChanged;
	}

	// Если диалог открыт из другого оконного диалога — владелец он, а не главное окно
	// VM без IWindowDialogSettings считаем модальными
	public void Open(IPage? masterPage) {
		var owner = (masterPage as IAvaloniaWindowPage)?.Window
			?? (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if(owner == null)
			Show();
		else if((page.ViewModel as IWindowDialogSettings)?.IsModal ?? true)
			_ = ShowDialog(owner);
		else
			Show(owner);
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
		page.ViewModel.PropertyChanged -= titleChanged;
		if(page.Window == null)
			return;

		page.Window = null;
		close(page);
	}
}
