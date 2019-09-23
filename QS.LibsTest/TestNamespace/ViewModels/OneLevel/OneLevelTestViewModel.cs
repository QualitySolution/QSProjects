using QS.Services;
using QS.ViewModels;

namespace QS.Test.TestNamespace.ViewModels.OneLevel
{
	public class OneLevelTestViewModel : TabViewModelBase
	{
		public OneLevelTestViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
