using QS.Launcher.ViewModels.PageViewModels;
using ReactiveUI;

namespace QS.Launcher.ViewModels {
	public class MainWindowVM : ViewModelBase {
		public int PagesCount { get; set; }

		LoginVM login;

		private int selectedPageIndex;
		public int SelectedPageIndex {
			get => selectedPageIndex;
			set => this.RaiseAndSetIfChanged(ref selectedPageIndex, value);
		}

		public MainWindowVM(IEnumerable<CarouselPageVM> pages) {

			foreach (var page in pages) {
				page.NextPageCommand = ReactiveCommand.Create(NextPage);
				page.PreviousPageCommand = ReactiveCommand.Create(PreviousPage);
				page.ChangePageCommand = ReactiveCommand.Create<int>(ChangePage);

				if(page is LoginVM loginVM)
					login = loginVM;
			}
		}

		public void SaveConnections() {
			login.SerialaizeConnections();
		}

		public void ChangePage(int index) {
			SelectedPageIndex = index;
		}

		public void NextPage() {
			ChangePage((SelectedPageIndex + 1) % PagesCount);
		}

		public void PreviousPage() {
			ChangePage((SelectedPageIndex - 1 + PagesCount) % PagesCount);
		}
	}
}
