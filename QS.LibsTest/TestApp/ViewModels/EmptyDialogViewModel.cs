using System;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Test.TestApp.ViewModels
{
	public class EmptyDialogViewModel : DialogViewModelBase
	{
		public EmptyDialogViewModel(INavigationManager navigation) : base(navigation)
		{
		}
	}
}
