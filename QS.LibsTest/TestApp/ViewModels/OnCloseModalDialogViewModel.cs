using System;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Test.TestApp.ViewModels
{
	public class OnCloseModalDialogViewModel : ModalDialogViewModelBase, IOnCloseActionViewModel
	{
		public OnCloseModalDialogViewModel(INavigationManager navigation) : base(navigation)
		{
		}

		public Action<CloseSource> OnCloseCall;

		public void OnClose(CloseSource closeSource)
		{
			OnCloseCall?.Invoke(closeSource);
		}
	}
}
