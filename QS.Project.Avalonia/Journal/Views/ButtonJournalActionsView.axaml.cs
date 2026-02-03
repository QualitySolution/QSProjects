using System;
using System.Collections.Specialized;
using Avalonia.Controls;
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
		private IButtonJournalActionsViewModel? viewModel;

		public ButtonJournalActionsView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			actionsPanelControl = this.FindControl<StackPanel>("actionsPanel");
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			base.OnDataContextChanged(e);

			// Отписываемся от старой ViewModel
			if (viewModel != null)
			{
				viewModel.ActionsView.CollectionChanged -= Actions_CollectionChanged;
			}

			// Подписываемся на новую ViewModel
			viewModel = DataContext as IButtonJournalActionsViewModel;
			
			if (viewModel != null)
			{
				viewModel.ActionsView.CollectionChanged += Actions_CollectionChanged;
				RebuildActions();
			}
		}

		private void Actions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			RebuildActions();
		}

		private void RebuildActions()
		{
			if (actionsPanelControl == null || viewModel == null)
				return;

			actionsPanelControl.Children.Clear();

			foreach (var action in viewModel.ActionsView)
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
				button.Click += (_, _) =>
				{
					action.Execute();
				};

				actionsPanelControl.Children.Add(button);
			}
		}
	}
}
