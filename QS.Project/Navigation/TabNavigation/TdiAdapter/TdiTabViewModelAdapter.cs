using System;
using QS.ViewModels;
using QS.Services;
namespace QS.Navigation.TabNavigation.TdiAdapter
{
	public class TdiTabViewModelAdapter : TabViewModelBase
	{
		public TdiTabViewModelAdapter(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
