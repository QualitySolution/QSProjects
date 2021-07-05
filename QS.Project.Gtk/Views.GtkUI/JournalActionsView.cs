using QS.ViewModels;

namespace QS.Views.GtkUI
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class JournalActionsView : ViewBase<JournalActionsViewModel>
    {
        public JournalActionsView(JournalActionsViewModel viewModel) : base(viewModel)
        {
            Build();
            Configure();
        }

        private void Configure()
        {
            btnSelect.Clicked += (sender, args) => ViewModel.SelectCommand.Execute();
                       
            btnSelect.Binding.AddBinding(ViewModel, vm => vm.CanSelect, w => w.Sensitive).InitializeFromSource();
			btnSelect.Binding.AddBinding(ViewModel, vm => vm.IsSelectVisible, w => w.Visible).InitializeFromSource();
        }
    }
}
