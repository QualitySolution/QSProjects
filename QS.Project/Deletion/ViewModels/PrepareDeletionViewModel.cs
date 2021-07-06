using System;
using System.Threading;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Deletion.ViewModels
{
	public class PrepareDeletionViewModel : WindowDialogViewModelBase, IOnCloseActionViewModel
	{
		internal readonly DeleteCore Deletion;
		private readonly CancellationTokenSource cancellation;

		public PrepareDeletionViewModel(DeleteCore deletion, INavigationManager navigation, CancellationTokenSource cancellation = null) : base(navigation)
		{
			this.Deletion = deletion ?? throw new ArgumentNullException(nameof(deletion));
			this.cancellation = cancellation;
			Title = "Подготовка к удалению";
		}

		public void CancelOperation()
		{
			Close(false, CloseSource.Cancel);
		}

		public void OnClose(CloseSource source	)
		{
			if(source == CloseSource.ClosePage || source == CloseSource.AppQuit || source == CloseSource.Cancel)
				cancellation?.Cancel();
		}
	}
}
