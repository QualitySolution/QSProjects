using QS.Services;
using QS.ViewModels;

namespace QS.Test.TestNamespace.ViewModels.Two.Level
{
	public class SecondTestClassViewModel : TabViewModelBase
	{
		public SecondTestClassViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
