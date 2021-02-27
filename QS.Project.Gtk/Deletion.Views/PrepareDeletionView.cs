using System;
using Gamma.Binding.Converters;
using QS.Deletion.ViewModels;
using QS.Utilities;

namespace QS.Deletion.Views
{
	public partial class PrepareDeletionView : Gtk.Bin
	{
		private readonly PrepareDeletionViewModel viewModel;

		public PrepareDeletionView(PrepareDeletionViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			viewModel.Deletion.PropertyChanged += Deletion_PropertyChanged;
			ylabelOperation.Binding.AddBinding(viewModel.Deletion, e => e.OperationTitle, w => w.Text).InitializeFromSource();
			var converter = new NumbersToStringConverter();
			ylabelToDelete.Binding.AddBinding(viewModel.Deletion, e => e.ItemsToDelete, w => w.Text, converter).InitializeFromSource();
			ylabelToClean.Binding.AddBinding(viewModel.Deletion, e => e.ItemsToClean, w => w.Text, converter).InitializeFromSource();
			ylabelToRemoveFrom.Binding.AddBinding(viewModel.Deletion, e => e.ItemsToRemoveFrom, w => w.Text, converter).InitializeFromSource();
			ylabelLinks.Binding.AddBinding(viewModel.Deletion, e => e.TotalLinks, w => w.Text, converter).InitializeFromSource();
		}

		void Deletion_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			GtkHelper.WaitRedraw();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			viewModel.CancelOperation();
		}
	}
}
