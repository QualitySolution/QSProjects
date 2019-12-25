using System;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;

namespace QS.Test.TestApp.ViewModels
{
	public class AbortCreationViewModel : DialogViewModelBase
	{
		public AbortCreationViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
			throw new AbortCreatingPageException("Вкладка не создана!", "Остановка");
		}
	}
}
