using System;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Dialog.ViewModels
{
	public class ProgressWindowViewModel : WindowDialogViewModelBase
	{
		public ProgressWindowViewModel(INavigationManager navigation) : base(navigation)
		{
			IsModal = true;
			Resizable = false;
			WindowPosition = WindowGravity.Center;
			Title = "Подождите...";
		}

		public IProgressBarDisplayable Progress { get; set; }
	}
}
