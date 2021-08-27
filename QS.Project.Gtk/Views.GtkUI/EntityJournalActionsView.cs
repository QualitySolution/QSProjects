using System.Linq;
using Gamma.GtkWidgets;
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
            CreateActions();
        }

        private void CreateActions()
        {
            if(ViewModel.JournalActions.Any())
            {
                foreach(var action in ViewModel.JournalActions)
                {
                    var btn = new yButton();
                    btn.Show();

                    btn.Clicked += (sender, args) => action.ExecuteAction?.Invoke();

                    btn.Binding.AddSource(action)
                        .AddBinding(a => a.Label, w => w.Label)
                        .AddBinding(a => a.Sensitive, w => w.Sensitive)
                        .AddBinding(a => a.Visible, w => w.Visible)
                        .InitializeFromSource();

                    yhboxBtns.Add(btn);
                }
            }
        }
    }
}
