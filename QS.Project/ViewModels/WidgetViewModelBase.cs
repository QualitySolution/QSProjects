using System;
using QS.Services;

namespace QS.ViewModels
{
	public abstract class WidgetViewModelBase : ViewModelBase
	{
		protected WidgetViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
