using QS.DomainModel.UoW;

namespace QS.ViewModels
{
	public class UoWWidgetViewModelBase : WidgetViewModelBase
	{
		public IUnitOfWork UoW { get; set; }
	}
}
