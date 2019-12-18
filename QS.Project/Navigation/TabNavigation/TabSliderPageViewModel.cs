using QS.Services;
using QS.ViewModels;
using System.Linq;
namespace QS.Navigation.TabNavigation
{
	public class TabSliderPageViewModel : TabPageViewModelBase
	{
		public IPage SlidedPage { get; }

		public TabSliderPageViewModel(IPage page, INavigationManager navigationManager, IInteractiveService interactiveService) : base(page, navigationManager, interactiveService)
		{
			SlidedPage = page.ChildPages.FirstOrDefault();
			if(SlidedPage == null) {
				return;
			}
			if(SlidedPage.ViewModel is TabViewModelBase tabViewModel) {
				tabViewModel.TabNameChanged += (sender, e) => UpdateSlidedTabTitle(tabViewModel.TabName);
				UpdateSlidedTabTitle(tabViewModel.TabName);
			}
		}

		private void UpdateSlidedTabTitle(string tabTitle)
		{
			Title = $"{base.Title} | {tabTitle}";
		}


		private string title;
		public override string Title {
			get => title;
			set => SetField(ref title, value, () => Title);
		}
	}
}
