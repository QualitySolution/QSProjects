using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Test.TestApp.ViewModels
{
	public class AbortCreationViewModel : DialogViewModelBase
	{
		public AbortCreationViewModel(INavigationManager navigation) : base(navigation)
		{
			throw new AbortCreatingPageException("Вкладка не создана!", "Остановка");
		}
	}
}
