using System;
using Gdk;
using Gtk;
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
		}
	}
}
