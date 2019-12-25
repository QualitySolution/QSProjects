using QS.Navigation;
using QS.ViewModels;

namespace QS.Test.TestApp.ViewModels
{
	public class AbortCreationViewModel : DialogViewModelBase
	{
		public AbortCreationViewModel()
		{
			throw new AbortCreatingPageException("Вкладка не создана!", "Остановка");
		}
	}
}
