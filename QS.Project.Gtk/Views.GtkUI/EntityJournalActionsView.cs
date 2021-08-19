using QS.ViewModels;

namespace QS.Views.GtkUI
{
    public partial class EntityJournalActionsView : ViewBase<EntityJournalActionsViewModel>
    {
        public EntityJournalActionsView(EntityJournalActionsViewModel viewModel) : base(viewModel)
        {
            Build();
            Configure();
        }

        private void Configure()
        {
            //btnSelect.Clicked += (sender, e) => ViewModel.SelectCommand.Execute();
            btnAdd.Clicked += (sender, e) => ViewModel.AddCommand.Execute();
            btnEdit.Clicked += (sender, e) => ViewModel.EditCommand.Execute();
            btnDelete.Clicked += (sender, e) => ViewModel.DeleteCommand.Execute();

            /*btnSelect.Binding.AddSource(ViewModel)
                .AddBinding(vm => vm.CanSelect, w => w.Sensitive)
                .AddBinding(vm => vm.IsSelectVisible, w => w.Visible)
                .InitializeFromSource();*/
            btnAdd.Binding.AddSource(ViewModel)
                .AddBinding(vm => vm.CanCreate, w => w.Sensitive)
                .AddBinding(vm => vm.IsAddVisible, w => w.Visible)
                .InitializeFromSource();
            btnEdit.Binding.AddSource(ViewModel)
                .AddBinding(vm => vm.CanEdit, w => w.Sensitive)
                .AddBinding(vm => vm.IsEditVisible, w => w.Visible)
                .InitializeFromSource();
            btnDelete.Binding.AddSource(ViewModel)
                .AddBinding(vm => vm.CanDelete, w => w.Sensitive)
                .AddBinding(vm => vm.IsDeleteVisible, w => w.Visible)
                .InitializeFromSource();
            
        }
    }
}
