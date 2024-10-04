using QS.Launcher.ViewModels.Commands;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class CarouselPageVM : ViewModelBase
	{
		public ICommand? NextPageCommand { get; set; }

		public ICommand? PreviousPageCommand { get; set; }

		public ICommand? ChangePageCommand { get; set; }

		public CarouselPageVM(NextPageCommand? nextPageCommand, PreviousPageCommand? previousPageCommand, ChangePageCommand? changePageCommand)
		{
			NextPageCommand = nextPageCommand;
			PreviousPageCommand = previousPageCommand;
			ChangePageCommand = changePageCommand;
		}

		public CarouselPageVM() { }	
	}
}
