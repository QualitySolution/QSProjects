using System;
using QS.Serial.ViewModels;

namespace QS.Serial.Views
{
	public partial class SerialNumberView : Gtk.Bin
	{
		private readonly SerialNumberViewModel viewModel;

		public SerialNumberView(SerialNumberViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

			serialnumberentry1.Binding.AddBinding(viewModel, vm => vm.SerialNumber, w => w.Text).InitializeFromSource();
			labelResult.Binding.AddBinding(viewModel, v => v.ResultText, w => w.LabelProp).InitializeFromSource();
			ybuttonApply.Binding.AddBinding(viewModel, v => v.SensetiveOk, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnYbuttonApplyClicked(object sender, EventArgs e)
		{
			viewModel.Save();
		}

		protected void OnYbuttonCancelClicked(object sender, EventArgs e)
		{
			viewModel.Close(false, Navigation.CloseSource.Cancel);
		}
	}
}

