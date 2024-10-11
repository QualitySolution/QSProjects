using System.Windows.Input;
using QS.ViewModels;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class CarouselPageVM : ViewModelBase {
		public ICommand NextPageCommand { get; set; }

		public ICommand PreviousPageCommand { get; set; }

		public ICommand ChangePageCommand { get; set; }
	}
}
