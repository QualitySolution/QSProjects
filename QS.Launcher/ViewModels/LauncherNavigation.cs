using System;
using System.Collections.ObjectModel;
using QS.Launcher.ViewModels.PageViewModels;
using QS.ViewModels;
using ReactiveUI;

namespace QS.Launcher.ViewModels {
	public class LauncherNavigation : ViewModelBase {
		private int rootPagesCount;

		public LauncherNavigation() {
			Pages.CollectionChanged += (sender, e) => this.RaisePropertyChanged(nameof(CurrentPage));
		}

		public ObservableCollection<CarouselPageVM> Pages { get; } = new ObservableCollection<CarouselPageVM>();

		private int selectedIndex;
		public int SelectedIndex {
			get => selectedIndex;
			set {
				this.RaiseAndSetIfChanged(ref selectedIndex, value);
				this.RaisePropertyChanged(nameof(CurrentPage));
			}
		}

		public CarouselPageVM CurrentPage =>
			selectedIndex >= 0 && selectedIndex < Pages.Count
				? Pages[selectedIndex]
				: null;

		public void SetRoots(params CarouselPageVM[] roots) {
			if(roots == null)
				throw new ArgumentNullException(nameof(roots));

			Pages.Clear();
			foreach(var page in roots)
				Pages.Add(page);

			rootPagesCount = Pages.Count;
			SelectedIndex = 0;
		}

		public void ChangePage(int index) {
			if(index < 0 || index >= Pages.Count)
				return;
			SelectedIndex = index;
		}

		public void Next() {
			if(rootPagesCount == 0)
				return;
			PopToRoot();
			ChangePage((SelectedIndex + 1) % rootPagesCount);
		}

		public void Previous() {
			if(rootPagesCount == 0)
				return;
			PopToRoot();
			ChangePage((SelectedIndex - 1 + rootPagesCount) % rootPagesCount);
		}

		/// <summary>добавить страницу поверх текущей и перейти на неё</summary>
		public void Push(CarouselPageVM page) {
			if(page == null)
				return;
			Pages.Add(page);
			SelectedIndex = Pages.Count - 1;
		}

		/// <summary>закрыть текущую нерутовую страницу и вернуться на предыдущую</summary>
		public void Pop() {
			if(Pages.Count <= rootPagesCount)
				return;
			int targetIndex = Pages.Count - 2;
			SelectedIndex = targetIndex;
			RemovePagesAbove(targetIndex);
		}

		/// <summary>закрыть все нерутовые страницы и вернуться к корневым вкладкам</summary>
		public void PopToRoot()
		{
			int targetIndex = rootPagesCount - 1;
			if(SelectedIndex > targetIndex)
				SelectedIndex = targetIndex;
			RemovePagesAbove(targetIndex);
		}

		/// <summary>Снимает всё, что стоит выше первой страницы указанного типа</summary>
		public void PopTo(Type pageType) {
			if(pageType == null)
				return;

			int targetIndex = -1;
			for(int i = 0; i < Pages.Count; i++) {
				if(pageType.IsInstanceOfType(Pages[i])) { targetIndex = i; break; }
			}
			if(targetIndex < 0)
				return;

			SelectedIndex = targetIndex;
			RemovePagesAbove(targetIndex);
		}

		private void RemovePagesAbove(int keepIndex)
		{
			while(Pages.Count > keepIndex + 1) {
				int last = Pages.Count - 1;
				var page = Pages[last];
				Pages.RemoveAt(last);
				(page as IDisposable)?.Dispose();
			}
		}
	}
}
