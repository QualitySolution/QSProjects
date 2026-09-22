using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using QS.Journal.Views;
using System;

namespace QS.Navigation;

public static class PageView {
	/// <summary>
	/// Заворачивает вью в прокрутку, если оно не умеет прокручиваться само
	/// </summary>
	public static Control Wrap(Control view) {
		// Журнал прокручивает таблицу сам. На бесконечной высоте, которую даёт ScrollViewer, DataGrid строит все строки разом и теряет прилипшую шапку
		if(view is JournalView)
			return view;

		return new ScrollViewer {
			Content = view,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
		};
	}

	public static void DisposeOnClose(IPage page) {
		var content = (page as IAvaloniaPage)?.View ?? (page as IAvaloniaWindowPage)?.View;
		if(content == null)
			return;

		// Обёртку прокрутки надела Wrap — она же тут и снимается
		var view = content is ScrollViewer wrapper && wrapper.Content is Control inner ? inner : content;
		(view as IDisposable)?.Dispose();
	}
}
