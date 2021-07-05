using QS.ViewModels;

namespace QS.Views.GtkUI
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EntityJournalActionsView : ViewBase<EntityJournalActionsViewModel>
    {
        public EntityJournalActionsView(EntityJournalActionsViewModel viewModel) : base(viewModel)
        {
            Build();
            Configure();
        }

        private void Configure()
        {
            btnSelect.Clicked += (sender, e) => ViewModel.SelectCommand.Execute();
            btnAdd.Clicked += (sender, e) => ViewModel.AddCommand.Execute();
            btnEdit.Clicked += (sender, e) => ViewModel.EditCommand.Execute();
            btnDelete.Clicked += (sender, e) => ViewModel.DeleteCommand.Execute();

            btnSelect.Binding.AddBinding(ViewModel, vm => vm.CanSelect, w => w.Sensitive).InitializeFromSource();
            btnSelect.Binding.AddBinding(ViewModel, vm => vm.IsSelectVisible, w => w.Visible).InitializeFromSource();
            btnAdd.Binding.AddBinding(ViewModel, vm => vm.CanCreate, w => w.Sensitive).InitializeFromSource();
            btnAdd.Binding.AddBinding(ViewModel, vm => vm.IsAddVisible, w => w.Visible).InitializeFromSource();
            btnEdit.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
            btnEdit.Binding.AddBinding(ViewModel, vm => vm.IsEditVisible, w => w.Visible).InitializeFromSource();
            btnDelete.Binding.AddBinding(ViewModel, vm => vm.CanDelete, w => w.Sensitive).InitializeFromSource();
            btnDelete.Binding.AddBinding(ViewModel, vm => vm.IsDeleteVisible, w => w.Visible).InitializeFromSource();
        }
    }
}
