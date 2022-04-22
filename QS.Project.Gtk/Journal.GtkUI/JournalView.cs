using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Autofac;
using Gdk;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Utilities;
using QS.Utilities.Text;
using QS.ViewModels;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using QS.Views.Resolve;
using QSWidgetLib;

namespace QS.Journal.GtkUI
{
	[WindowSize(900, 600)]
	public partial class JournalView : TabViewBase<JournalViewModelBase>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		#region Глобальные настройки

		public static uint ShowProgressbarDelay = 800;
		public static uint ProgressPulseTime = 100;
		public static bool ThrowExceptionOnDataLoad = true;

		#endregion

		public JournalView(JournalViewModelBase viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureJournal();
			CreateTextSpinner();
			KeyPressEvent += JournalView_KeyPressEvent;
		}

		private Widget FilterView;
		private void ConfigureJournal()
		{
			ViewModel.DataLoader.ItemsListUpdated += ViewModel_ItemsListUpdated;
			ViewModel.DataLoader.LoadingStateChanged += DataLoader_LoadingStateChanged;
			ViewModel.DataLoader.TotalCountChanged += DataLoader_TotalCountChanged;
			if(ThrowExceptionOnDataLoad)
				ViewModel.DataLoader.LoadError += DataLoader_LoadError;
			checkShowFilter.Clicked += (sender, e) => { hboxFilter.Visible = checkShowFilter.Active; };
			buttonRefresh.Clicked += (sender, e) => { ViewModel.Refresh(); };
			tableview.ButtonReleaseEvent += Tableview_ButtonReleaseEvent;
			tableview.Selection.Changed += Selection_Changed;
			SetSeletionMode(ViewModel.SelectionMode);
			ConfigureActions();

			//FIXME Этот код только для водовоза
			var filterProp = ViewModel.GetType().GetProperty("Filter");
			if(DialogHelper.FilterWidgetResolver != null && filterProp != null && filterProp.GetValue(ViewModel) is IJournalFilter filter) {
				Widget filterWidget = DialogHelper.FilterWidgetResolver.Resolve(filter);
				FilterView = filterWidget;
				hboxFilter.Add(filterWidget);
				filterWidget.Show();
				checkShowFilter.Visible = true;
				checkShowFilter.Active = hboxFilter.Visible = !filter.HidenByDefault;
			}

			if(ViewModel.JournalFilter is ViewModelBase filterViewModel) {
				var viewResolver = ViewModel.AutofacScope.Resolve<IGtkViewResolver>();
				FilterView = viewResolver.Resolve(filterViewModel);
				if(FilterView == null)
					throw new InvalidOperationException($"Не найдена View для {filterViewModel.GetType()}");
				hboxFilter.Add(FilterView);
				FilterView.Show();
				checkShowFilter.Visible = true;
				checkShowFilter.Active = hboxFilter.Visible = ViewModel.JournalFilter.IsShow;
				ViewModel.JournalFilter.PropertyChanged += JournalFilter_PropertyChanged;
			}

			Widget searchView = ViewModel.AutofacScope != null ? ResolutionExtensions.ResolveOptionalNamed<Widget>(ViewModel.AutofacScope, "GtkJournalSearchView", new TypedParameter(typeof(SearchViewModel), ViewModel.Search)) : null;
			//FIXME В будущем надо бы наверно полностью отказаться от создания SearchView здесь в ручную.
			if(searchView == null)
				searchView = new SearchView((SearchViewModel)ViewModel.Search);
			hboxSearch.Add(searchView);
			searchView.Show();

			tableview.ColumnsConfig = TreeViewColumnsConfigFactory.Resolve(ViewModel);
			GtkScrolledWindow.Vadjustment.ValueChanged += Vadjustment_ValueChanged;

			tableview.ItemsDataSource = ViewModel.Items;
			ViewModel.Refresh();
			UpdateButtons();
			SetTotalLableText();
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
			ViewModel.UpdateJournalActions += UpdateButtons;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.FooterInfo))
				Application.Invoke((s, args) => labelFooter.Markup = ViewModel.FooterInfo);
			if(e.PropertyName == nameof(ViewModel.SelectionMode))
				SetSeletionMode(ViewModel.SelectionMode);
		}

		void JournalFilter_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.JournalFilter.IsShow))
				checkShowFilter.Active = hboxFilter.Visible = ViewModel.JournalFilter.IsShow;
		}

		TextSpinner CountingTextSpinner;

		void CreateTextSpinner()
		{
			ISpinnerTemplate timeOfDaySpinner = DateTime.Now.TimeOfDay < new TimeSpan(6, 30, 0) || DateTime.Now.TimeOfDay > new TimeSpan(18, 30, 0)
				? (ISpinnerTemplate)new SpinnerTemplateMoon() : new SpinnerTemplateClock();
			var whishList = new ISpinnerTemplate[] {
				timeOfDaySpinner,
				new SpinnerTemplateDotsScrolling()
			};

			foreach(var spinner in whishList) {
				var allChars = String.Join("", spinner.Frames);
				var layout = labelTotalRow.CreatePangoLayout(allChars);
				//К сожалению этот способ определения не поддерживаемых символов на винде для нецветных спинеров все равно возвращает 0, даже если символ не поддерживается.
				if(layout.UnknownGlyphsCount == 0) {
					CountingTextSpinner = new TextSpinner(spinner);
					break;
				}
				logger.Debug($"Спинер {spinner.GetType()} пропущен, так как используемый шрифт не поддеживает {layout.UnknownGlyphsCount} из {new StringInfo(allChars).LengthInTextElements} используемых символов.");
			}
		}

		void SetSeletionMode(JournalSelectionMode mode)
		{
			switch(mode) {
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
		}

		#region События загрузчика данных

		void ViewModel_ItemsListUpdated(object sender, EventArgs e)
		{
			Application.Invoke((s, arg) => {
				if(isDestroyed)
					return;
					
				labelFooter.Markup = ViewModel.FooterInfo;
				tableview.SearchHighlightTexts = ViewModel.Search.SearchValues;
				tableview.ItemsDataSource = ViewModel.DataLoader.Items;

				if(!ViewModel.DataLoader.FirstPage) {
					GtkHelper.WaitRedraw();
					if(GtkScrolledWindow?.Vadjustment != null)
						GtkScrolledWindow.Vadjustment.Value = lastScrollPosition;
				}
			});
		}

		private LoadingState loadingState;

		void DataLoader_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			if(loadingState != LoadingState.InProgress && e.LoadingState == LoadingState.InProgress)
				GLib.Timeout.Add(ShowProgressbarDelay, new GLib.TimeoutHandler(StartLoadProgress));
			loadingState = e.LoadingState;
		}

		bool StartLoadProgress()
		{
			progressbarLoading.Visible = loadingState == LoadingState.InProgress;

			if(loadingState == LoadingState.InProgress) {
				GLib.Timeout.Add(ProgressPulseTime, new GLib.TimeoutHandler(PulseProgress));
			} 
			return false;
		}

		bool PulseProgress()
		{
			if(loadingState == LoadingState.Idle) {
				progressbarLoading.Visible = false;
				return false;
			} else {
				progressbarLoading.Pulse();
				return true;
			}
		}

		void DataLoader_LoadError(object sender, LoadErrorEventArgs e)
		{
			Application.Invoke((s, ea) => throw e.Exception);
		}

		#endregion

		#region Отображение общего количества строк

		void SetTotalLableText()
		{
			labelTotalRow.Markup = GetTotalRowText();
		}

		void DataLoader_TotalCountChanged(object sender, EventArgs e)
		{
			if(!ViewModel.DataLoader.TotalCountingInProgress) {
				Application.Invoke((s, arg) => SetTotalLableText());
			}
		}

		bool UpdateTotalCount()
		{
			SetTotalLableText();
			return ViewModel.DataLoader.TotalCountingInProgress;
		}

		string GetTotalRowText()
		{
			if(ViewModel.DataLoader.TotalCountingInProgress) {
				if(ViewModel.DataLoader.TotalCount.HasValue)
					return $"Всего: {CountingTextSpinner.GetFrame()}{ViewModel.DataLoader.TotalCount}";
				else
					return $"Всего: {CountingTextSpinner.GetFrame()}";
			} else {
				if(ViewModel.DataLoader.TotalCount.HasValue)
					return $"Всего: {ViewModel.DataLoader.TotalCount}";
				else
					return "Всего: <span foreground=\"blue\" underline=\"single\">???</span>";
			}
		}

		protected void OnEventboxTotalRowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			GLib.Timeout.Add(CountingTextSpinner.RecommendedInterval, new GLib.TimeoutHandler(UpdateTotalCount));
			ViewModel.DataLoader.GetTotalCount();
		}

		#endregion

		double lastScrollPosition;

		void Vadjustment_ValueChanged(object sender, EventArgs e)
		{
			if(!ViewModel.DataLoader.DynamicLoadingEnabled || GtkScrolledWindow.Vadjustment.Value + GtkScrolledWindow.Vadjustment.PageSize < GtkScrolledWindow.Vadjustment.Upper)
				return;

			if(ViewModel.DataLoader.HasUnloadedItems) {
				lastScrollPosition = GtkScrolledWindow.Vadjustment.Value;
				ViewModel.DataLoader.LoadData(true);
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

			tableview.RowActivated += (o, args) =>
			{
				var selectedItems = GetSelectedItems();
				if(ViewModel.RowActivatedAction != null && ViewModel.RowActivatedAction.GetSensitivity(selectedItems))
				{
					ViewModel.RowActivatedAction.ExecuteAction(selectedItems);
				}
			};
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

		void Tableview_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right || !ViewModel.PopupActions.Any())
			{
				return;
			}

			var popupMenu = new Menu();
			foreach(var popupAction in ViewModel.PopupActions)
			{
				CreatePopupMenuActionsRecursively(popupMenu, popupAction, GetSelectedItems());
			}

			if(popupMenu.Children.Length == 0)
			{
				return;
			}

			popupMenu.Show();
			popupMenu.Popup();
		}

		private static void CreatePopupMenuActionsRecursively(Menu menu, IJournalAction popupAction, object[] selected)
		{
			var item = new MenuItem(popupAction.Title);
			item.Sensitive = popupAction.GetSensitivity(selected);
			item.Visible = popupAction.GetVisibility(selected);

			if(popupAction.ChildActions.Any())
			{
				var subMenu = new Menu();
				foreach(var childAction in popupAction.ChildActions)
				{
					CreatePopupMenuActionsRecursively(subMenu, childAction, selected);
				}
				item.Submenu = subMenu;
			}
			//Действия выполняются только для самых последних дочерних JournalActions
			else
			{
				item.ButtonPressEvent += (o, args) =>
				{
					if(args.Event.Button == (uint)GtkMouseButton.Left)
					{
						popupAction.ExecuteAction(selected);
					}
				};
			}
			menu.Add(item);
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

		private bool isDestroyed = false;
		public override void Destroy()
		{
			isDestroyed = true;
			ViewModel.DataLoader.CancelLoading();
			FilterView?.Destroy();
			base.Destroy();
		}

		private void JournalView_KeyPressEvent(object o, KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.F5) {
				ViewModel.Refresh();
				return;
			}

			ExecuteTypeJournalAction(args.Event.Key.ToString());
		}

		private void ExecuteTypeJournalAction(string hotKey)
		{
			var nodeActions = ViewModel.NodeActions
				.Where(n => !string.IsNullOrWhiteSpace(n.HotKeys)
					&& n.HotKeys
						.Replace(" ", string.Empty).ToLower()
						.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
						.Contains(hotKey.ToLower()))
				.ToList();

			if(nodeActions.Count > 1)
			{
				throw new InvalidOperationException($"Должен быть только один NodeAction с горячей клавишей {hotKey}");
			}

			if(nodeActions.Count == 1)
			{
				var selectedItems = GetSelectedItems();
				var action = nodeActions.First();
				if(action.GetSensitivity(selectedItems) && action.GetVisibility(selectedItems))
				{
					action.ExecuteAction.Invoke(selectedItems);
				}
			}
		}

	}

	public enum GtkMouseButton
	{
		Left = 1,
		Middle = 2,
		Right = 3
	}
}
