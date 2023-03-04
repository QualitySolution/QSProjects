using System;
using QS.DBScripts.ViewModels;
using QS.Views.Dialog;

namespace QS.DBScripts.Views
{
	public partial class AdminLoginView : DialogViewBase<AdminLoginViewModel>
	{
		public AdminLoginView(AdminLoginViewModel viewModel) : base(viewModel)
		{
			this.Build();
			entryLogin.Binding.AddBinding(ViewModel, v => v.Login, w => w.Text).InitializeFromSource();
			entryPassword.Binding.AddBinding(ViewModel, v => v.Password, w => w.Text).InitializeFromSource();
			if(!String.IsNullOrWhiteSpace(ViewModel.Login))
				entryPassword.GrabFocus();
		}

		protected void OnButtonOkClicked(object sender, EventArgs e)
		{
			ViewModel.Accept();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Cancel();
		}

		protected void OnEntryLoginActivated(object sender, EventArgs e)
		{
			this.ChildFocus(Gtk.DirectionType.TabForward);
		}

		protected void OnEntryPasswordActivated(object sender, EventArgs e)
		{
			ViewModel.Accept();
		}
	}
}
