using System;
using System.ComponentModel;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.ViewModels;

namespace QS.Dialog.Views {
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public class StatusBarView : Statusbar {
		yLabel labelUser;
		yLabel labelStatus;
		
		public StatusBarView() {
			Remove(Children[0]);
			labelUser = new yLabel();
			labelUser.Xalign = 0;
			PackStart(labelUser, false, false, 0);
			PackStart(new VSeparator(), false, false, 0);
			labelStatus = new yLabel();
			labelStatus.Xalign = 1;
			labelStatus.ExposeEvent += (o, args) => statusBarRedrawHandled = true;
			PackEnd(labelStatus, true, true, 0);
		}
		
		private StatusBarViewModel viewModel;
		public StatusBarViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				labelUser.Binding.AddBinding(viewModel, vm => vm.UserName, w => w.LabelProp).InitializeFromSource();
				labelStatus.Binding.AddBinding(viewModel, vm => vm.StatusText, w => w.LabelProp).InitializeFromSource();
				ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			}
		}
		
		bool statusBarRedrawHandled;

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			statusBarRedrawHandled = false;
			while (Application.EventsPending () && !statusBarRedrawHandled) {
				Main.Iteration();
			}
		}
	}
}
