using System;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Extension;

namespace QS.ViewModels.Dialog
{
	public abstract class WindowDialogViewModelBase : DialogViewModelBase, IWindowDialogSettings
	{
		public bool IsModal { get; protected set; } = true;
		public bool EnableMinimizeMaximize { get; protected set; } = false;
		public WindowGravity WindowPosition { get; protected set; }

		protected WindowDialogViewModelBase(INavigationManager navigation) : base(navigation)
		{
		}
	}
}
