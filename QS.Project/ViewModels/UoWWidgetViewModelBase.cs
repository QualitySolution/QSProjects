using System;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.ViewModels
{
	public class UoWWidgetViewModelBase : WidgetViewModelBase
	{
		public IUnitOfWork UoW { get; set; }

		public UoWWidgetViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
