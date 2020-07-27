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

