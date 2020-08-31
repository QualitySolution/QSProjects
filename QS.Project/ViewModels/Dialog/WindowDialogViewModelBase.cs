using System;
using QS.Dialog;
using QS.Navigation;

namespace QS.ViewModels.Dialog
{
	public abstract class WindowDialogViewModelBase : DialogViewModelBase
	{
		public bool IsModal { get; protected set; } = true;
		public WindowGravity WindowPosition { get; protected set; }

		protected WindowDialogViewModelBase(INavigationManager navigation) : base(navigation)
		{
		}
	}
}
