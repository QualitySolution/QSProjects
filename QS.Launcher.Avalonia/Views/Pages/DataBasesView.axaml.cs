using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Util;
using QS.Launcher.ViewModels.PageViewModels;
using System;
using System.Linq;

namespace QS.Launcher.Views.Pages;
public partial class DataBasesView : UserControl {
	public DataBasesView(DataBasesVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
		viewModel.StartClosingLauncherEvent += () => (App.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Shutdown();
	}

	public void Label_PointerPressed(object? sender, PointerPressedEventArgs e) {
		if(!adminPanel.Classes.Remove("invisible"))
			adminPanel.Classes.Add("invisible");
	}
}
