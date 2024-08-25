using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;

public class CarouselPageVM(ICommand? nextPageCommand, ICommand? previousPageCommand, ICommand? changePageCommand) : ViewModelBase
{
	protected ICommand? nextPageCommand;
	public ICommand? NextPageCommand { get; } = nextPageCommand;

	protected ICommand? previousPageCommand;
	public ICommand? PreviousPageCommand { get; } = previousPageCommand;

	protected ICommand? changePageCommand;
	public ICommand? ChangePageCommand { get; } = changePageCommand;
}
