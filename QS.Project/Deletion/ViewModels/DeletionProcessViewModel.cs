using System;
using System.Threading;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Deletion.ViewModels
{
	public class DeletionProcessViewModel : WindowDialogViewModelBase, IOnCloseActionViewModel
	{
		internal readonly DeleteCore Deletion;
		private readonly CancellationTokenSource cancellation;

		public DeletionProcessViewModel(INavigationManager navigation, DeleteCore deletion, CancellationTokenSource cancellation = null) : base(navigation)
		{
			this.Deletion = deletion ?? throw new ArgumentNullException(nameof(deletion));
			this.cancellation = cancellation;
			Title = "Удаление";
		}

		public void CancelOperation()
		{
			Close(false, CloseSource.Cancel);
		}

		public void OnClose(CloseSource source)
		{
			if(source == CloseSource.ClosePage || source == CloseSource.AppQuit || source == CloseSource.Cancel) {
				try {
					cancellation?.Cancel();
				}
				catch(ObjectDisposedException) { }
			}
		}
	}
}
