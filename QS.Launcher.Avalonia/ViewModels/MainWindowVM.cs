using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.ViewModels.PageViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace QS.Launcher.ViewModels {
	public class MainWindowVM : ViewModelBase {
		public int PagesCount { get; set; }

		LoginVM login;

		private int selectedPageIndex;
		public int SelectedPageIndex {
			get => selectedPageIndex;
			set => this.RaiseAndSetIfChanged(ref selectedPageIndex, value);
		}

		public MainWindowVM(DataBasesVM dataBasesVM, LoginVM loginVM, BaseManagementVM baseManagementVM, UserManagementVM userManagementVM
			, IServiceProvider provider)
		{
			CarouselPageVM[] pages = { dataBasesVM, loginVM, baseManagementVM, userManagementVM };
			foreach (var page in pages) {
				page.NextPageCommand = ReactiveCommand.Create(NextPage);
				page.PreviousPageCommand = ReactiveCommand.Create(PreviousPage);
				page.ChangePageCommand = ReactiveCommand.Create<int>(ChangePage);
			}
			login = loginVM;
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
