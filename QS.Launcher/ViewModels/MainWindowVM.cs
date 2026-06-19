using System;
using System.Collections.ObjectModel;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using QS.ViewModels;
using ReactiveUI;

namespace QS.Launcher.ViewModels {
	/// <summary>
	/// Держит коллекцию страниц для Carousel и индекс активной
	/// </summary>
	public class MainWindowVM : ViewModelBase {
		LoginVM login;
		private readonly int rootPagesCount;

		public ObservableCollection<CarouselPageVM> Pages { get; }

		private int selectedPageIndex;
		public int SelectedPageIndex {
			get => selectedPageIndex;
			set => this.RaiseAndSetIfChanged(ref selectedPageIndex, value);
		}

		public int PagesCount {
			get => rootPagesCount;
			set { }
		}

		public MainWindowVM(
			DataBasesVM dataBasesVM,
			LoginVM loginVM,
			UserManagementVM userManagementVM,
			IServiceProvider provider)
		{
			Pages = new ObservableCollection<CarouselPageVM> {
				loginVM, dataBasesVM
			};
			rootPagesCount = Pages.Count;
			login = loginVM;

			foreach(var page in Pages)
				WirePage(page);
		}

		private void WirePage(CarouselPageVM page) {
			page.NextPageCommand = ReactiveCommand.Create(NextPage);
			page.PreviousPageCommand = ReactiveCommand.Create(PreviousPage);
			page.ChangePageCommand = ReactiveCommand.Create<int>(ChangePage);
			page.PushPageCommand = ReactiveCommand.Create<CarouselPageVM>(PushPage);
			page.PopPageCommand = ReactiveCommand.Create(PopPage);
			page.PopToRootCommand = ReactiveCommand.Create(PopToRoot);
			page.PopToPageCommand = ReactiveCommand.Create<Type>(type => {
				var method = GetType()
					.GetMethod(nameof(PopToPage))
					.MakeGenericMethod(type);

				method.Invoke(this, null);
			});
		}

		public void SaveConnections() {
			login.SaveConnections();
		}

		public void ChangePage(int index) {
			if(index < 0 || index >= Pages.Count) return;
			SelectedPageIndex = index;
		}

		public void NextPage() {
			PopToRoot();
			ChangePage((SelectedPageIndex + 1) % rootPagesCount);
		}

		public void PreviousPage() {
			PopToRoot();
			ChangePage((SelectedPageIndex - 1 + rootPagesCount) % rootPagesCount);
		}

		public void PushPage(CarouselPageVM page) {
			if(page == null) return;
			WirePage(page);
			Pages.Add(page);
			SelectedPageIndex = Pages.Count - 1;
		}

		public void PopPage() {
			if(Pages.Count <= rootPagesCount) return;
			int last = Pages.Count - 1;
			Pages.RemoveAt(last);
			SelectedPageIndex = Pages.Count - 1;
		}

		public void PopToRoot() {
			while(Pages.Count > rootPagesCount)
				Pages.RemoveAt(Pages.Count - 1);
			if(SelectedPageIndex >= rootPagesCount)
				SelectedPageIndex = rootPagesCount - 1;
		}

		public void PopToPage<TPage>() where TPage : CarouselPageVM {
			int targetIdx = -1;
			for(int i = 0; i < Pages.Count; i++) {
				if(Pages[i] is TPage) { targetIdx = i; break; }
			}
			if(targetIdx < 0) return;
			while(Pages.Count > targetIdx + 1)
				Pages.RemoveAt(Pages.Count - 1);
			SelectedPageIndex = targetIdx;
		}
	}
}
