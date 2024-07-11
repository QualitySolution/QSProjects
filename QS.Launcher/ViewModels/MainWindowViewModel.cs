using Avalonia.Controls;
using QS.Launcher.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.Launcher.ViewModels; 
public class MainWindowViewModel : ViewModelBase
{
	public List<UserControl> Pages { get; set; }

	private UserControl selectedPage;
	public UserControl SelectedPage
	{
		get => selectedPage;
		set => this.RaiseAndSetIfChanged(ref selectedPage, value);
	}

	public MainWindowViewModel()
	{
		Pages =
		[
			new LoginView(),
			new DataBasesView()
		];

		SelectedPage = Pages[0];
	}

	public void ChangePage(int index)
	{
		SelectedPage = Pages[index];
	}

	public void NextPage()
	{
		SelectedPage = Pages[(Pages.IndexOf(SelectedPage) + 1) % Pages.Count];
	}

	public void PreviousPage()
	{
		SelectedPage = Pages[(Pages.IndexOf(SelectedPage) - 1 + Pages.Count) % Pages.Count];
	}
}
