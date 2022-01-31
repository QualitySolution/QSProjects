using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace QS.HistoryLog.ViewModels
{
	public class HistoryViewModel : UowDialogViewModelBase
	{
		public HistoryViewModel(IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, IValidator validator = null, 
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle)
		{
			Title = "Просмотр журнала изменений";
		}
	}
}
