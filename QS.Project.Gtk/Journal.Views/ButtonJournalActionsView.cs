using System;
using Gtk;
using QS.Journal.Actions;

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
			Build();
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
				var button = CreateButton(action);
				hboxButtons.PackStart(button, false, false, 0);
			}

			hboxButtons.ShowAll();
		}

		private Widget CreateButton(IJournalActionView action)
		{
			var button = new Button();
			button.Label = action.Title;
			button.Sensitive = action.Sensitive;
			button.Visible = action.Visible;

			// Подписываемся на изменения свойств действия
			action.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(action.Title):
						button.Label = action.Title;
						break;
					case nameof(action.Sensitive):
						button.Sensitive = action.Sensitive;
						break;
					case nameof(action.Visible):
						button.Visible = action.Visible;
						break;
				}
			};

			// Подписываемся на клик кнопки
			button.Clicked += (sender, e) => action.Execute();

			return button;
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
