using System;
using QS.Views.GtkUI;
using QS.Project.Journal;
using NLog;
using Gtk;
using QSWidgetLib;
using System.Linq;
using System.Collections.Generic;
using QS.Dialog.Gtk;
using QS.Project.Search.GtkUI;
using QS.Project.Search;
using QS.Helpers;

namespace QS.Journal.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class JournalView : TabViewBase<JournalViewModelBase>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public JournalView(JournalViewModelBase viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureJournal();
		}

		private void ConfigureJournal()
		{
			ViewModel.ItemsListUpdated += ViewModel_ItemsListUpdated;
			checkShowFilter.Clicked += (sender, e) => { hboxFilter.Visible = checkShowFilter.Active; };
			buttonRefresh.Clicked += (sender, e) => { ViewModel.Refresh(); };
			tableview.ButtonReleaseEvent += Tableview_ButtonReleaseEvent;
			tableview.Selection.Changed += Selection_Changed;
			switch(ViewModel.SelectionMode) {
				case JournalSelectionMode.None:
				case JournalSelectionMode.Single:
					tableview.Selection.Mode = SelectionMode.Single;
					break;
				case JournalSelectionMode.Multiple:
					tableview.Selection.Mode = SelectionMode.Multiple;
					break;
				default:
					tableview.Selection.Mode = SelectionMode.None;
					break;
			}
			ConfigureActions();

			if(ViewModel.Filter != null) {
				Widget filterWidget = DialogHelper.FilterWidgetResolver.Resolve(ViewModel.Filter);
				hboxFilter.Add(filterWidget);
				filterWidget.Show();
				checkShowFilter.Visible = true;
				checkShowFilter.Active = true;
			}

			var searchView = new SearchView((SearchViewModel)ViewModel.Search);
			hboxSearch.Add(searchView);
			searchView.Show();

			tableview.ColumnsConfig = TreeViewColumnsConfigFactory.Resolve(ViewModel.GetType());
			GtkScrolledWindow.Vadjustment.ValueChanged += Vadjustment_ValueChanged;

			tableview.ItemsDataSource = ViewModel.Items;
			RefreshSource();
			UpdateButtons();
		}

		void ViewModel_ItemsListUpdated(object sender, EventArgs e)
		{
			if(!ViewModel.DynamicLoadingEnabled) {
				tableview.ItemsDataSource = ViewModel.Items;
			}

			Application.Invoke((s, arg) => {
				labelFooter.LabelProp = ViewModel.FooterInfo;
				tableview.YTreeModel.EmitModelChanged();
			});
		}

		private void RefreshSource()
		{
			ViewModel.Refresh();
		}

		bool takenAll = false;
		bool isAdjustmentValueChanged = false;

		void Vadjustment_ValueChanged(object sender, EventArgs e)
		{
			if(!ViewModel.DynamicLoadingEnabled  || GtkScrolledWindow.Vadjustment.Value + GtkScrolledWindow.Vadjustment.PageSize < GtkScrolledWindow.Vadjustment.Upper)
				return;

			if(!takenAll) {
				var lastScrollPosition = GtkScrolledWindow.Vadjustment.Value;
				takenAll = !ViewModel.TryLoad();
				GtkHelper.WaitRedraw();
				GtkScrolledWindow.Vadjustment.Value = lastScrollPosition;
			}
		}

		private object[] GetSelectedItems()
		{
			return tableview.GetSelectedObjects();
		}

		List<System.Action> actionsSensitivity;
		List<System.Action> actionsVisibility;

		private void ConfigureActions()
		{
			if(actionsSensitivity == null) {
				actionsSensitivity = new List<System.Action>();
			} else {
				actionsSensitivity.Clear();
			}

			if(actionsVisibility == null) {
				actionsVisibility = new List<System.Action>();
			} else {
				actionsVisibility.Clear();
			}

			foreach(var action in ViewModel.NodeActions) {
				Widget actionWidget;

				if(action.ChildActions.Any()) {
					MenuButton menuButton = new MenuButton();
					menuButton.Label = action.Title;
					Menu childActionButtons = new Menu();
					foreach(var childAction in action.ChildActions) {
						childActionButtons.Add(CreateMenuItemWidget(childAction));
					}
					menuButton.Menu = childActionButtons;
					actionWidget = menuButton;
				} else {
					Button button = new Button();
					button.Label = action.Title;

					button.Clicked += (sender, e) => { action.ExecuteAction(GetSelectedItems()); };

					actionsSensitivity.Add(() => {
						button.Sensitive = action.GetSensitivity(GetSelectedItems());
					});

					actionsVisibility.Add(() => {
						button.Visible = action.GetVisibility(GetSelectedItems());
					});
					actionWidget = button;
				}

				actionWidget.ShowAll();

				hboxButtons.Add(actionWidget);
				Box.BoxChild addDocumentButtonBox = (Box.BoxChild)hboxButtons[actionWidget];
				addDocumentButtonBox.Expand = false;
				addDocumentButtonBox.Fill = false;
			}

			tableview.RowActivated += (o, args) => { ViewModel.RowActivatedAction.ExecuteAction(GetSelectedItems()); };
		}	

		private MenuItem CreateMenuItemWidget(IJournalAction action)
		{
			MenuItem menuItem = new MenuItem(action.Title);

			menuItem.Activated += (sender, e) => {
				action.ExecuteAction(GetSelectedItems());
			};

			actionsSensitivity.Add(() => {
				menuItem.Sensitive = action.GetSensitivity(GetSelectedItems());
			});

			actionsVisibility.Add(() => {
				menuItem.Visible = action.GetVisibility(GetSelectedItems());
			});

			if(action.ChildActions.Any()) {
				foreach(var childAction in action.ChildActions) {
					menuItem.Add(CreateMenuItemWidget(childAction));
				}
			}

			return menuItem;
		}

		private Menu popupMenu;

		void Tableview_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3 && ViewModel.PopupActions.Any()) {
				var selected = GetSelectedItems();
				if(popupMenu == null) {
					popupMenu = new Menu();
					foreach(var popupAction in ViewModel.PopupActions) {
						var item = new MenuItem(popupAction.Title);
						item.Sensitive = popupAction.GetSensitivity(selected);
						item.Visible = popupAction.GetVisibility(selected);
						item.Activated += (sender, e) => { popupAction.ExecuteAction(selected); };
						if(popupAction.ChildActions.Any()) {
							foreach(var childAction in popupAction.ChildActions) {
								item.Add(CreatePopupMenuItem(childAction));
							}
						}
						popupMenu.Add(item);
					}
				}

				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}

		private MenuItem CreatePopupMenuItem(IJournalAction journalAction)
		{
			MenuItem menuItem = new MenuItem(journalAction.Title);
			menuItem.Activated += (sender, e) => { journalAction.ExecuteAction(GetSelectedItems()); };
			menuItem.Sensitive = journalAction.GetSensitivity(GetSelectedItems());
			menuItem.Visible = journalAction.GetVisibility(GetSelectedItems());
			foreach(var childAction in journalAction.ChildActions) {
				menuItem.Add(CreatePopupMenuItem(childAction));
			}
			return menuItem;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			UpdateButtons();
		}

		private void UpdateButtons()
		{
			if(actionsSensitivity != null) {
				foreach(var item in actionsSensitivity) {
					item.Invoke();
				}
			}
			if(actionsVisibility != null) {
				foreach(var item in actionsVisibility) {
					item.Invoke();
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			ViewModel.Dispose();
		}
	}
}
