using QS.HistoryLog.Domain;
using QS.HistoryLog.ViewModels;
using QS.Views.Dialog;

namespace QS.HistoryLog.Views
{
	public partial class HistoryView : DialogViewBase<HistoryViewModel>
	{
		public HistoryView(HistoryViewModel viewModel): base(viewModel)
		{
			this.Build();
			ycomboUsers.Binding.AddBinding(viewModel, v => v.Users, w => w.Active).InitializeFromSource();
			ycomboObjects.Binding.AddBinding(viewModel, v => v.ChangeObjects, w => w.Active).InitializeFromSource();
			ycomboAction.ItemsEnum = typeof(EntityChangeOperation);
			ycomboAction.Binding.AddBinding(viewModel, v => v.ChangeOperation, w => w.SelectedItem).InitializeFromSource();


		}
	}
}