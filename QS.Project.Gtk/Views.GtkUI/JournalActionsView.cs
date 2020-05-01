using System;
using QS.Project.Journal;
namespace QS.Views.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class JournalActionsView : ViewBase<JournalActionsViewModel>
	{
		public JournalActionsView(JournalActionsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{

			btnAdd.Clicked += (sender, e) => ViewModel.AddCommand.Execute();
			btnEdit.Clicked += (sender, e) => ViewModel.EditCommand.Execute();
			btnDelete.Clicked += (sender, e) => ViewModel.DeleteCommand.Execute();
			btnSelect.Clicked += (sender, e) => ViewModel.SelectCommand.Execute();

			btnAdd.Binding.AddBinding(ViewModel, vm => vm.AddButtonVisibility, v => v.Visible).InitializeFromSource();
			btnAdd.Binding.AddBinding(ViewModel, vm => vm.AddButtonSensitivity, v => v.Sensitive).InitializeFromSource();
			btnEdit.Binding.AddBinding(ViewModel, vm => vm.EditButtonVisibility, v => v.Visible).InitializeFromSource();
			btnEdit.Binding.AddBinding(ViewModel, vm => vm.EditButtonSensitivity, v => v.Sensitive).InitializeFromSource();
			btnDelete.Binding.AddBinding(ViewModel, vm => vm.DeleteButtonVisibility, v => v.Visible).InitializeFromSource();
			btnDelete.Binding.AddBinding(ViewModel, vm => vm.DeleteButtonSensitivity, v => v.Sensitive).InitializeFromSource();
			btnSelect.Binding.AddBinding(ViewModel, vm => vm.SelectButtonVisibility, v => v.Visible).InitializeFromSource();
			btnSelect.Binding.AddBinding(ViewModel, vm => vm.SelectButtonSensitivity, v => v.Sensitive).InitializeFromSource();
		}
	}
}
