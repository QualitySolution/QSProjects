using System.Windows.Input;

namespace QS.Launcher.ViewModels;

public class PageViewModel(ICommand nextPageCommand, ICommand previousPageCommand) : ViewModelBase
{
	protected ICommand? nextPageCommand;
	public ICommand? NextPageCommand { get; } = nextPageCommand;

	protected ICommand? previousPageCommand;
	public ICommand? PreviousPageCommand { get; } = previousPageCommand;
}
