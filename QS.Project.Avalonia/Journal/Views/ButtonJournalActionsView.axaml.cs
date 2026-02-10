using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using QS.Journal.Actions;

namespace QS.Journal.Views
{
	/// <summary>
	/// View для кнопочной панели действий журнала
	/// </summary>
	public partial class ButtonJournalActionsView : UserControl
	{
		private StackPanel? actionsPanelControl;
		private StackPanel? rightActionsPanelControl;
		private IButtonJournalActionsViewModel? viewModel;

		public ButtonJournalActionsView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			actionsPanelControl = this.FindControl<StackPanel>("actionsPanel");
			rightActionsPanelControl = this.FindControl<StackPanel>("rightActionsPanel");
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);

			// Отписываемся от старой ViewModel
			if (viewModel != null)
			{
				viewModel.LeftActionsView.CollectionChanged -= LeftActions_CollectionChanged;
				viewModel.RightActionsView.CollectionChanged -= RightActions_CollectionChanged;
			}

			// Подписываемся на новую ViewModel
			viewModel = DataContext as IButtonJournalActionsViewModel;
			
			if (viewModel != null)
			{
				viewModel.LeftActionsView.CollectionChanged += LeftActions_CollectionChanged;
				viewModel.RightActionsView.CollectionChanged += RightActions_CollectionChanged;
				RebuildLeftActions();
				RebuildRightActions();
			}
		}

		private void LeftActions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			RebuildLeftActions();
		}

		private void RightActions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			RebuildRightActions();
		}

		private void RebuildLeftActions()
		{
			if (actionsPanelControl == null || viewModel == null)
				return;

			actionsPanelControl.Children.Clear();

			foreach (var action in viewModel.LeftActionsView)
			{
				Control control;
				
				// Если есть дочерние действия - создаем DropDownButton
				if (action.ChildActionsView != null && action.ChildActionsView.Any())
				{
					control = CreateDropDownButton(action);
				}
				// Иначе создаем обычную кнопку
				else
				{
					control = CreateButton(action);
				}

				actionsPanelControl.Children.Add(control);
			}
		}

		private void RebuildRightActions()
		{
			if (rightActionsPanelControl == null || viewModel == null)
				return;

			rightActionsPanelControl.Children.Clear();

			foreach (var action in viewModel.RightActionsView)
			{
				Control control;
				
				// Если есть дочерние действия - создаем DropDownButton
				if (action.ChildActionsView != null && action.ChildActionsView.Any())
				{
					control = CreateDropDownButton(action);
				}
				// Иначе создаем обычную кнопку
				else
				{
					control = CreateButton(action);
				}

				rightActionsPanelControl.Children.Add(control);
			}
		}

		private Button CreateButton(IJournalActionView action)
		{
			var button = new Button();
			
			// Создаем биндинги программно
			button.Bind(Button.ContentProperty, 
				new Avalonia.Data.Binding(nameof(action.Title)) { Source = action });
			button.Bind(Button.IsEnabledProperty, 
				new Avalonia.Data.Binding(nameof(action.Sensitive)) { Source = action });
			button.Bind(Button.IsVisibleProperty, 
				new Avalonia.Data.Binding(nameof(action.Visible)) { Source = action });

			// Обработчик клика
			button.Click += (_, _) => action.Execute();

			return button;
		}

		private DropDownButton CreateDropDownButton(IJournalActionView action)
		{
			var dropDownButton = new DropDownButton();
			
			// Создаем биндинги программно
			dropDownButton.Bind(DropDownButton.ContentProperty, 
				new Avalonia.Data.Binding(nameof(action.Title)) { Source = action });
			dropDownButton.Bind(DropDownButton.IsEnabledProperty, 
				new Avalonia.Data.Binding(nameof(action.Sensitive)) { Source = action });
			dropDownButton.Bind(DropDownButton.IsVisibleProperty, 
				new Avalonia.Data.Binding(nameof(action.Visible)) { Source = action });

			// Создаем меню с дочерними действиями
			var menuFlyout = new MenuFlyout();
			foreach (var childAction in action.ChildActionsView)
			{
				var menuItem = CreateMenuItem(childAction);
				menuFlyout.Items.Add(menuItem);
			}
			
			dropDownButton.Flyout = menuFlyout;

			return dropDownButton;
		}

		private MenuItem CreateMenuItem(IJournalActionView action)
		{
			var menuItem = new MenuItem();
			
			// Создаем биндинги программно
			menuItem.Bind(HeaderedItemsControl.HeaderProperty, 
				new Avalonia.Data.Binding(nameof(action.Title)) { Source = action });
			menuItem.Bind(MenuItem.IsEnabledProperty, 
				new Avalonia.Data.Binding(nameof(action.Sensitive)) { Source = action });
			menuItem.Bind(MenuItem.IsVisibleProperty, 
				new Avalonia.Data.Binding(nameof(action.Visible)) { Source = action });

			// Если есть дочерние действия - создаем подменю
			if (action.ChildActionsView != null && action.ChildActionsView.Any())
			{
				foreach (var childAction in action.ChildActionsView)
				{
					menuItem.Items.Add(CreateMenuItem(childAction));
				}
			}
			// Иначе привязываем выполнение действия
			else
			{
				menuItem.Click += (_, _) => action.Execute();
			}

			return menuItem;
		}
	}
}
