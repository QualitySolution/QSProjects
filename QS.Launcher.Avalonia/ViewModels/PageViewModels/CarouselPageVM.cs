using QS.Launcher.ViewModels.Commands;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;

public class CarouselPageVM(NextPageCommand? nextPageCommand, PreviousPageCommand? previousPageCommand, ChangePageCommand? changePageCommand) : ViewModelBase
{
	protected ICommand? nextPageCommand;
	public ICommand? NextPageCommand { get; } = nextPageCommand;

	protected ICommand? previousPageCommand;
	public ICommand? PreviousPageCommand { get; } = previousPageCommand;

	protected ICommand? changePageCommand;
	public ICommand? ChangePageCommand { get; } = changePageCommand;
}
