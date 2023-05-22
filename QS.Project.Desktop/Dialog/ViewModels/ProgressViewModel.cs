using System;
using System.Threading;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Dialog.ViewModels
{
	public class ProgressWindowViewModel : WindowDialogViewModelBase, IOnCloseActionViewModel
	{
		public ProgressWindowViewModel(INavigationManager navigation, bool userCanCancel = false) : base(navigation)
		{
			IsModal = true;
			Resizable = false;
			Deletable = userCanCancel;
			WindowPosition = WindowGravity.Center;
			Title = "Подождите...";

			if(userCanCancel) {
				CancellationTokenSource = new CancellationTokenSource();
			}
		}

		public CancellationTokenSource CancellationTokenSource { get; private set; }
		public IProgressBarDisplayable Progress { get; set; }

		public void OnClose(CloseSource source) {
			if(source == CloseSource.ClosePage)
				CancellationTokenSource?.Cancel();
		}
	}
}
