﻿using System;
using Gtk;
using QS.Dialog.GtkUI;

namespace QS.ErrorReporting
{
	public partial class ErrorMessageView : Gtk.Dialog
	{
		public ErrorMessageView(ErrorMessageViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			this.Build();
			this.SetPosition(WindowPosition.CenterOnParent);
			ConfigureDlg();
		}

		ErrorMessageViewModel ViewModel;

		void ConfigureDlg()
		{
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			entryEmail.Binding.AddBinding(ViewModel, vm => vm.Email, w => w.Text).InitializeFromSource();

			textviewError.Binding.AddFuncBinding(ViewModel, vm => (vm as ErrorMessageViewModel).ExceptionText, w => w.Buffer.Text).InitializeFromSource();
			textviewDescription.Binding.AddBinding(ViewModel, vm => vm.Description, w => w.Buffer.Text).InitializeFromSource();

			ybuttonSendReport.Clicked += (sender, e) => ViewModel.SendReportCommand.Execute(ErrorReportType.User); 
			ybuttonSendReport.Binding.AddBinding(ViewModel, vm => vm.CanSendErrorReportManually, w => w.Sensitive).InitializeFromSource();

			ybuttonCopy.Clicked += YbuttonCopy_Clicked;
			yButtonOK.Clicked += (sender, e) => this.Destroy();
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.IsEmailValid)) {
				if(ViewModel.IsEmailValid)
					entryEmail.ModifyText(StateType.Normal);
				else
					entryEmail.ModifyText(StateType.Normal, new Gdk.Color(255, 0, 0));
			}
		}

		void YbuttonCopy_Clicked(object sender, EventArgs e)
		{
			Clipboard clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
			clipboard.Text = ViewModel.ErrorData;
			clipboard.Store();
		}

		protected override void OnDestroyed()
		{
			ViewModel.SendReportAutomatically();
		}
	}
}

