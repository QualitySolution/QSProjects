using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Project.Journal;
using QS.ViewModels;
using QS.Widgets;

namespace QS.Views.GtkUI
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EntitiesJournalActionsView : ViewBase<EntitiesJournalActionsViewModel>
    {
        public EntitiesJournalActionsView(EntitiesJournalActionsViewModel viewModel) : base(viewModel)
        {
            Build();
            Configure();
        }

		private void Configure()
		{
			btnSelect.Clicked += (sender, args) => ViewModel.SelectCommand.Execute();
			btnEdit.Clicked += (sender, args) => ViewModel.EditCommand.Execute();
			btnDelete.Clicked += (sender, args) => ViewModel.DeleteCommand.Execute();

			btnSelect.Binding.AddBinding(ViewModel, vm => vm.CanSelect, w => w.Sensitive).InitializeFromSource();
			btnSelect.Binding.AddBinding(ViewModel, vm => vm.IsSelectVisible, w => w.Visible).InitializeFromSource();
			btnEdit.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			btnEdit.Binding.AddBinding(ViewModel, vm => vm.IsEditVisible, w => w.Visible).InitializeFromSource();
			btnDelete.Binding.AddBinding(ViewModel, vm => vm.CanDelete, w => w.Sensitive).InitializeFromSource();
			btnDelete.Binding.AddBinding(ViewModel, vm => vm.IsDeleteVisible, w => w.Visible).InitializeFromSource();

			if(ViewModel.IsAddVisible)
			{
				CreateAddButton();
			}
		}
		
		private void CreateAddButton()
		{
			Widget actionWidget;

			if (ViewModel.AddJournalActions != null) 
			{
				var menuButton = new MenuButton 
				{
					Label = ViewModel.AddJournalActions.Title
				};

				var childActionButtons = new Menu();

				foreach (var childAction in ViewModel.AddJournalActions.ChildActions) 
				{
					childActionButtons.Add(CreateMenuItemWidget(childAction));
				}

				menuButton.Menu = childActionButtons;
				actionWidget = menuButton;
			}
			else 
			{
				var addButton = new yButton 
				{
					Label = "Добавить"
				};
				
				addButton.Clicked += (sender, e) => ViewModel.AddCommand.Execute();
				addButton.Binding.AddBinding(ViewModel, vm => vm.CanAdd, w => w.Sensitive).InitializeFromSource();
				addButton.Binding.AddBinding(ViewModel, vm => vm.IsAddVisible, w => w.Visible).InitializeFromSource();
				
				actionWidget = addButton;
			}

			actionWidget.Show();
			yhboxAddBtn.Add(actionWidget);
		}

		private MenuItem CreateMenuItemWidget(IJournalAction action)
		{
			MenuItem menuItem = new MenuItem(action.Title);

			menuItem.Activated += (sender, e) => 
			{
				action.ExecuteAction(ViewModel.SelectedItems);
			};

			menuItem.Sensitive = action.GetSensitivity(ViewModel.SelectedItems);

			menuItem.Visible = action.GetVisibility(ViewModel.SelectedItems);

			if (action.ChildActions.Any()) 
			{
				foreach (var childAction in action.ChildActions) 
				{
					menuItem.Add(CreateMenuItemWidget(childAction));
				}
			}

			return menuItem;
		}
	}
}
