using System;
using QS.BaseParameters.ViewModels;

namespace QS.BaseParameters.Views
{
	public partial class BaseParametersView : Gtk.Dialog
	{
		private readonly BaseParametersViewModel viewModel;

		public BaseParametersView (BaseParametersViewModel viewModel)
		{
			this.Build ();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

			treeParameters.CreateFluentColumnsConfig<ParameterValue>()
				.AddColumn("Название").AddTextRenderer(x => x.Name).Editable()
				.AddColumn("Значение").AddTextRenderer(x => x.ValueStr).Editable()
				.Finish();

			treeParameters.ItemsDataSource = viewModel.ParameterValues;

			buttonDelete.Sensitive = false;

			treeParameters.Selection.Changed += (sender, e) => {
				buttonDelete.Sensitive = (treeParameters.Selection.CountSelectedRows () > 0);
			};
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			viewModel.Save();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			viewModel.RemoveParameter(treeParameters.GetSelectedObject<ParameterValue>());
			RefreshTable();
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			viewModel.AddParameter();
			RefreshTable();
		}

		private void RefreshTable()
		{
			treeParameters.ItemsDataSource = viewModel.ParameterValues;
		}
	}
}

