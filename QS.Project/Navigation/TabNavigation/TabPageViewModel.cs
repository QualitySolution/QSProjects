using System;
using QS.Services;

namespace QS.Navigation.TabNavigation
{
	public class TabPageViewModel : TabPageViewModelBase
	{
		public TabPageViewModel(IPage page, INavigationManager navigationManager, IInteractiveService interactiveService) : base(page, navigationManager, interactiveService)
		{
		}
	}
}
