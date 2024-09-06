using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.Views.Pages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QS.Launcher.ViewModels; 
public class MainWindowVM : ViewModelBase
{
	public ObservableCollection<ContentControl> Pages { get; set; }

	LoginView login;

	private ContentControl selectedPage;
	public ContentControl SelectedPage
	{
		get => selectedPage;
		set => this.RaiseAndSetIfChanged(ref selectedPage, value);
	}

	public MainWindowVM(IServiceProvider serviceProvider)
	{
		login = serviceProvider.GetRequiredService<LoginView>();
		var databases = serviceProvider.GetRequiredService<DataBasesView>();
		var userManagement = serviceProvider.GetRequiredService<UserManagementView>();
		var baseManagement = serviceProvider.GetRequiredService<BaseManagementView>();

		Pages =
		[
			login, databases, userManagement, baseManagement
		];
	}

	public void SaveConnections() {
		((LoginVM)login.DataContext).SerialaizeConnections();
	}
	
	public void ChangePage(int index)
	{
		SelectedPage = Pages[index];
	}

	public void NextPage()
	{
		ChangePage((Pages.IndexOf(SelectedPage) + 1) % Pages.Count);
	}

	public void PreviousPage()
	{
		ChangePage((Pages.IndexOf(SelectedPage) - 1 + Pages.Count) % Pages.Count);
	}
}
