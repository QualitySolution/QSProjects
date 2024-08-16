using Avalonia.Controls;
using QS.Launcher.Views;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QS.Launcher.ViewModels; 
public class MainWindowViewModel : ViewModelBase
{
	public ObservableCollection<ContentControl> Pages { get; set; }

	private ContentControl selectedPage;
	public ContentControl SelectedPage
	{
		get => selectedPage;
		set => this.RaiseAndSetIfChanged(ref selectedPage, value);
	}

	private readonly ICommand nextPageCommand;
	private readonly ICommand previousPageCommand;

	public MainWindowViewModel(Carousel carousel)
	{
		nextPageCommand = ReactiveCommand.Create(NextPage);
		previousPageCommand = ReactiveCommand.Create(PreviousPage);

		var lw = new LoginView(nextPageCommand, previousPageCommand);
		var dbw = new DataBasesView(nextPageCommand, previousPageCommand);

		Pages =
		[
			lw, dbw
		];
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
