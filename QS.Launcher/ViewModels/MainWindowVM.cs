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
			set {
				this.RaiseAndSetIfChanged(ref selectedPageIndex, value);
				this.RaisePropertyChanged(nameof(CurrentPage));
			}
		}

		public CarouselPageVM CurrentPage =>
			selectedPageIndex >= 0 && selectedPageIndex < Pages.Count
				? Pages[selectedPageIndex]
				: null;

		public MainWindowVM(
			DataBasesVM dataBasesVM,
			LoginVM loginVM,
			IServiceProvider provider)
		{
			Pages = new ObservableCollection<CarouselPageVM> {
				loginVM, dataBasesVM
			};
			rootPagesCount = Pages.Count;
			login = loginVM;

			foreach(var page in Pages)
				WirePage(page);

			Pages.CollectionChanged += (sender, e) => this.RaisePropertyChanged(nameof(CurrentPage));
		}

		private void WirePage(CarouselPageVM page) {
			page.NextPageCommand = ReactiveCommand.Create(NextPage);
			page.PreviousPageCommand = ReactiveCommand.Create(PreviousPage);
			page.ChangePageCommand = ReactiveCommand.Create<int>(ChangePage);
			page.PushPageCommand = ReactiveCommand.Create<CarouselPageVM>(PushPage);
			page.PopPageCommand = ReactiveCommand.Create(PopPage);
			page.PopToRootCommand = ReactiveCommand.Create(PopToRoot);
			page.PopToPageCommand = ReactiveCommand.Create<Type>(PopToPage);
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
			RemovePagesAbove(Pages.Count - 2);
			SelectedPageIndex = Pages.Count - 1;
		}

		public void PopToRoot() {
			RemovePagesAbove(rootPagesCount - 1);
			if(SelectedPageIndex >= rootPagesCount)
				SelectedPageIndex = rootPagesCount - 1;
		}

		private void RemovePagesAbove(int keepIndex) {
			if(SelectedPageIndex > keepIndex)
				SelectedPageIndex = keepIndex;

			while(Pages.Count > keepIndex + 1) {
				int last = Pages.Count - 1;
				var page = Pages[last];
				Pages.RemoveAt(last);
				(page as IDisposable)?.Dispose();
			}
		}

		/// <summary>Снимает всё, что стоит выше первой страницы указанного типа</summary>
		public void PopToPage(Type pageType) {
			if(pageType == null) return;

			int targetIdx = -1;
			for(int i = 0; i < Pages.Count; i++) {
				if(pageType.IsInstanceOfType(Pages[i])) { targetIdx = i; break; }
			}
			if(targetIdx < 0) return;
			RemovePagesAbove(targetIdx);
			SelectedPageIndex = targetIdx;
		}
	}
}
