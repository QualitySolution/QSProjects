using QS.Launcher.ViewModels.PageViewModels;
using ReactiveUI;
using System;
using QS.ViewModels;

namespace QS.Launcher.ViewModels {
	public class MainWindowVM : ViewModelBase {
		public int PagesCount { get; set; }

		LoginVM login;
		private readonly DataBasesVM dataBases;

		private int selectedPageIndex;
		public int SelectedPageIndex {
			get => selectedPageIndex;
			set {
				if(selectedPageIndex == value) return;
				this.RaiseAndSetIfChanged(ref selectedPageIndex, value);
				dataBases?.SetPageActive(value == 1);
			}
		}

		public MainWindowVM(DataBasesVM dataBasesVM, LoginVM loginVM, BaseManagementVM baseManagementVM, UserManagementVM userManagementVM
			, IServiceProvider provider)
		{
			dataBases = dataBasesVM;
			CarouselPageVM[] pages = { dataBasesVM, loginVM, baseManagementVM, userManagementVM };
			foreach (var page in pages) {
				page.NextPageCommand = ReactiveCommand.Create(NextPage);
				page.PreviousPageCommand = ReactiveCommand.Create(PreviousPage);
				page.ChangePageCommand = ReactiveCommand.Create<int>(ChangePage);
			}
			login = loginVM;
		}

		public void SaveConnections() {
			login.SaveConnections();
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
