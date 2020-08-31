using System;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Test.TestApp.ViewModels
{
	public class ModalDialogViewModel : WindowDialogViewModelBase
	{
		public ModalDialogViewModel(INavigationManager navigation) : base(navigation)
		{
		}
	}
}
