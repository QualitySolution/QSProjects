using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.ViewModels;
using QS.Widgets;

namespace QS.Views.GtkUI
{
    public partial class EntitiesJournalActionsView : ViewBase<EntitiesJournalActionsViewModel>
    {
        public EntitiesJournalActionsView(EntitiesJournalActionsViewModel viewModel) : base(viewModel)
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
					if(action.ChildButtonElements.Any())
					{
						CreateMultipleButton(action.Label, action.ChildButtonElements);
					}
					else
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
		
		private void CreateMultipleButton(string label, IList<DefaultJournalAction> childActions)
		{
			var menuButton = new MenuButton 
			{
				Label = label
			};

			var childActionButtons = new Menu();

			foreach(var childAction in childActions) 
			{
				childActionButtons.Add(CreateMenuItemWidget(childAction));
			}

			menuButton.Menu = childActionButtons;
			menuButton.Show();
            yhboxBtns.Add(menuButton);
		}

		private MenuItem CreateMenuItemWidget(DefaultJournalAction action)
		{
			MenuItem menuItem = new MenuItem(action.Label);

			menuItem.Activated += (sender, e) => action.ExecuteAction?.Invoke();

			menuItem.Sensitive = action.Sensitive;

			menuItem.Visible = action.Visible;

			if(action.ChildButtonElements.Any())
			{
				foreach(var childAction in action.ChildButtonElements)
				{
					menuItem.Add(CreateMenuItemWidget(childAction));
				}
			}

			return menuItem;
		}
	}
}
