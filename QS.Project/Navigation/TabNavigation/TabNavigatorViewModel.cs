using System;
using QS.Services;
using QS.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace QS.Navigation.TabNavigation
{
	public class TabNavigatorViewModel : ViewModelBase
	{
		private readonly IInteractiveService interactiveService;

		private ObservableCollection<TabPageViewModelBase> pageViewModels;
		private List<TabPageViewModelBase> pageViewModelsStorage;

		public TabNavigator TabNavigator { get; }
		public ReadOnlyObservableCollection<TabPageViewModelBase> PageViewModels { get; set; }

		public TabNavigatorViewModel(TabNavigator tabNavigator, IInteractiveService interactiveService) : base(interactiveService)
		{
			TabNavigator = tabNavigator ?? throw new ArgumentNullException(nameof(tabNavigator));
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			TabNavigator.PageChanged += (sender, e) => UpdatePageViewModels();
			pageViewModels = new ObservableCollection<TabPageViewModelBase>();
			PageViewModels = new ReadOnlyObservableCollection<TabPageViewModelBase>(pageViewModels);
			pageViewModelsStorage = new List<TabPageViewModelBase>();
		}

		private TabPageViewModel activePageViewModel;
		public virtual TabPageViewModel ActivePageViewModel {
			get => activePageViewModel;
			set => SetField(ref activePageViewModel, value);
		}

		private void UpdatePageViewModels()
		{
			pageViewModels.Clear();
			foreach(var page in TabNavigator.TopLevelPages) {
				var pageViewModel = pageViewModelsStorage.FirstOrDefault(x => x.Page == page);
				if(pageViewModel == null) {
					pageViewModel = CreatePageViewModel(page);
				}
				pageViewModels.Add(pageViewModel);
			}
		}

		private TabPageViewModelBase CreatePageViewModel(IPage page)
		{
			TabPageViewModelBase pageViewModel;
			if(page.ChildPages.Any()) {
				pageViewModel = new TabSliderPageViewModel(page, TabNavigator, interactiveService);
			} else {
				pageViewModel = new TabPageViewModel(page, TabNavigator, interactiveService);
			}
			pageViewModelsStorage.Add(pageViewModel);
			pageViewModel.Page.PageClosed += (sender, e) => {
				if(pageViewModelsStorage.Contains(pageViewModel)) {
					pageViewModelsStorage.Remove(pageViewModel);
				}
			};

			return pageViewModel;
		}
	}
}
