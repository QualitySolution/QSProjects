using System;
using QS.Deletion.ViewModels;

namespace QS.Deletion.Views
{
	public partial class DeletionProcessView : Gtk.Bin
	{
		private readonly DeletionProcessViewModel viewModel;

		public DeletionProcessView(DeletionProcessViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			ylabelText.Binding.AddBinding(viewModel.Deletion, d => d.OperationTitle, w => w.LabelProp).InitializeFromSource();
			yprogressOperation.Adjustment = new Gtk.Adjustment(0, 0, 0, 1, 1, 1);
			yprogressOperation.Binding.AddSource(viewModel.Deletion)
				.AddBinding(d => d.ProgressUpper, w => w.Adjustment.Upper)
				.AddBinding(d => d.ProgressValue, w => w.Adjustment.Value)
				.InitializeFromSource();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			viewModel.CancelOperation();
		}
	}
}
