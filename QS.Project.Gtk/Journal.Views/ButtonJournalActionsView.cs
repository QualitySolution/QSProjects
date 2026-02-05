using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Journal.Actions;
using QS.Widgets;

namespace QS.Journal.Views {
	/// <summary>
	/// GTK View для кнопочной панели действий журнала
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ButtonJournalActionsView : Bin
	{
		private IButtonJournalActionsViewModel viewModel;

		public ButtonJournalActionsView()
		{
			this.Build();
		}

		public ButtonJournalActionsView(IButtonJournalActionsViewModel viewModel) : this()
		{
			ViewModel = viewModel;
		}

		public IButtonJournalActionsViewModel ViewModel
		{
			get => viewModel;
			set
			{
				if (viewModel != null)
				{
					viewModel.ActionsView.CollectionChanged -= ActionsView_CollectionChanged;
				}

				viewModel = value;

				if (viewModel != null)
				{
					viewModel.ActionsView.CollectionChanged += ActionsView_CollectionChanged;
					RebuildButtons();
				}
			}
		}

		private void ActionsView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			RebuildButtons();
		}

		private void RebuildButtons()
		{
			// Очищаем существующие кнопки
			foreach (Widget child in hboxButtons.Children)
			{
				hboxButtons.Remove(child);
				child.Destroy();
			}

			if (viewModel == null)
				return;

			// Создаем кнопки для каждого действия
			foreach (var action in viewModel.ActionsView)
			{
				var widget = CreateButtonWidget(action);
				hboxButtons.PackStart(widget, false, false, 0);
			}

			hboxButtons.ShowAll();
		}

		private Widget CreateButtonWidget(IJournalActionView action)
		{
			// Если есть дочерние действия - создаем MenuButton
			if (action.ChildActionsView != null && action.ChildActionsView.Any()) {
				return CreateMenuButton(action);
			}
			// Иначе создаем обычную кнопку
			else {
				return CreateButton(action);
			}
		}

		private Widget CreateButton(IJournalActionView action)
		{
			var button = new yButton();
			button.Binding.AddSource(action)
				.AddBinding(a => a.Title, w => w.Label)
				.AddBinding(a => a.Sensitive, w => w.Sensitive)
				.AddBinding(a => a.Visible, w => w.Visible)
				.InitializeFromSource();

			// Подписываемся на клик кнопки
			button.Clicked += (sender, e) => action.Execute();

			return button;
		}

		private Widget CreateMenuButton(IJournalActionView action)
		{
			var menuButton = new MenuButton();
			
			// Создаем меню с дочерними действиями
			var menu = new Menu();
			foreach (var childAction in action.ChildActionsView)
			{
				var menuItem = CreateMenuItem(childAction);
				menu.Add(menuItem);
			}
			menuButton.Menu = menu;

			// Биндинг свойств через Fluent API
			menuButton.Binding.AddSource(action)
				.AddBinding(a => a.Title, w => w.Label)
				.AddBinding(a => a.Sensitive, w => w.Sensitive)
				.AddBinding(a => a.Visible, w => w.Visible)
				.InitializeFromSource();

			return menuButton;
		}

		private MenuItem CreateMenuItem(IJournalActionView action)
		{
			var menuItem = new yMenuItem(action.Title);
			
			// Биндинг свойств через Fluent API
			menuItem.Binding.AddSource(action)
				.AddBinding(a => a.Title, w => w.Label)
				.AddBinding(a => a.Sensitive, w => w.Sensitive)
				.AddBinding(a => a.Visible, w => w.Visible)
				.InitializeFromSource();

			// Если есть дочерние действия - создаем подменю
			if (action.ChildActionsView != null && action.ChildActionsView.Any())
			{
				var subMenu = new Menu();
				foreach (var childAction in action.ChildActionsView)
				{
					subMenu.Add(CreateMenuItem(childAction));
				}
				menuItem.Submenu = subMenu;
			}
			// Иначе привязываем выполнение действия
			else
			{
				menuItem.Activated += (sender, e) => action.Execute();
			}

			return menuItem;
		}

		public override void Destroy()
		{
			if (viewModel != null)
			{
				viewModel.ActionsView.CollectionChanged -= ActionsView_CollectionChanged;
			}

			base.Destroy();
		}
	}
}
