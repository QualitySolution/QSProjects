using System;
using QS.Services;

namespace QS.ViewModels
{
	public abstract class WidgetViewModelBase : ViewModelBase
	{
		public WidgetViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
