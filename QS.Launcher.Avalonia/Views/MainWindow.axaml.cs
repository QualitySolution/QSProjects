using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views;

public partial class MainWindow : Window
{
	private readonly PageViewLocator viewLocator;

	public MainWindow(MainWindowVM vm, PageViewLocator viewLocator, LauncherOptions options) {
		InitializeComponent();

		this.viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));

		Icon = new WindowIcon(new Bitmap(new System.IO.MemoryStream(options.LogoIcon)));
		Title = options.AppTitle;

		Closing += (_, _) => vm.SaveConnections();

		// корневые страницы:
		foreach(var page in vm.Pages)
			carousel.Items.Add(viewLocator.Resolve(page));

		vm.Pages.CollectionChanged += OnPagesChanged;

		DataContext = vm;
	}

	/// <summary>
	/// Поддерживает carousel.Items в соответствии со стеком <see cref="MainWindowVM.Pages"/>
	/// </summary>
	private void OnPagesChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		switch(e.Action) {
			case NotifyCollectionChangedAction.Add:
				for(int i = 0; i < e.NewItems!.Count; i++)
					carousel.Items.Insert(e.NewStartingIndex + i,
						viewLocator.Resolve((CarouselPageVM)e.NewItems[i]!));
				break;

			case NotifyCollectionChangedAction.Remove:
				for(int i = 0; i < e.OldItems!.Count; i++)
					carousel.Items.RemoveAt(e.OldStartingIndex);
				break;

			case NotifyCollectionChangedAction.Reset:
				carousel.Items.Clear();
				foreach(var page in (IEnumerable<CarouselPageVM>)sender!)
					carousel.Items.Add(viewLocator.Resolve(page));
				break;
		}
	}
}
