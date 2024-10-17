using System;
using Gdk;
using QS.DbManagement;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Views;

namespace QS.Launcher.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LoginView : ViewBase<LoginVM> {
		public LoginView(LoginVM viewModel) : base(viewModel)
		{
			this.Build();

			imageLogo.Binding.AddFuncBinding<LoginVM>(ViewModel, v => new Pixbuf(v.CompanyImage), w => w.Pixbuf).InitializeFromSource();
			labelAppName.Binding.AddBinding(ViewModel, v => v.AppTitle, w => w.LabelProp).InitializeFromSource();
			entryPassword.Binding.AddBinding(ViewModel, v => v.Password, w => w.Text).InitializeFromSource();
			comboboxConnections.SetRenderTextFunc<Connection>(c => c.ConnectionTitle);
			comboboxConnections.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Connections, w => w.ItemsList)
				.AddBinding(v => v.SelectedConnection, w => w.SelectedItem)
				.InitializeFromSource();
		}

		protected void OnButtonLoginClicked(object sender, EventArgs e) {
			ViewModel.LoginCommand.Execute(null);
		}

		protected void OnEntryPasswordActivated(object sender, EventArgs e) {
			buttonLogin.Activate();
		}
	}
}
