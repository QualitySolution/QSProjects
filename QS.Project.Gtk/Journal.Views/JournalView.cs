using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Journal.Actions;
using QS.Journal.GtkUI;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities;
using QS.Utilities.Text;
using QS.ViewModels;
using QS.Views.Dialog;
using QS.Views.Resolve;
using QS.Widgets;

namespace QS.Journal.Views
{
	[WindowSize(900, 600)]
	public partial class JournalView : DialogViewBase<JournalViewModelBase>
	{
		private readonly IGtkViewResolver viewResolver;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private bool isRedrawInProgress;

		#region Глобальные настройки

		public static uint ShowProgressbarDelay = 800;
		public static uint ProgressPulseTime = 100;
		public static bool ThrowExceptionOnDataLoad = true;

		#endregion

		public JournalView(JournalViewModelBase viewModel, IGtkViewResolver viewResolver = null) : base(viewModel)
		{
			this.viewResolver = viewResolver;
			this.Build();
			ConfigureJournal();
			CreateTextSpinner();
			KeyPressEvent += JournalView_KeyPressEvent;
		}

		private Widget FilterView;
		private ButtonJournalActionsView buttonActionsView;
		
		private void ConfigureJournal()
		{
			ViewModel.DataLoader.ItemsListUpdated += ViewModel_ItemsListUpdated;
			ViewModel.DataLoader.LoadingStateChanged += DataLoader_LoadingStateChanged;
			ViewModel.DataLoader.TotalCountChanged += DataLoader_TotalCountChanged;
			if(ThrowExceptionOnDataLoad)
				ViewModel.DataLoader.LoadError += DataLoader_LoadError;
			checkShowFilter.Clicked += (sender, e) => { ViewModel.IsFilterShow = checkShowFilter.Active; };
			buttonRefresh.Clicked += (sender, e) => { ViewModel.Refresh(); };
			tableview.ButtonReleaseEvent += Tableview_ButtonReleaseEvent;
			tableview.Selection.Changed += Selection_Changed;
			SetSelectionMode(ViewModel.TableSelectionMode);
			ConfigureActions();

			if(ViewModel.JournalFilter is ViewModelBase filterViewModel) {
				if(viewResolver == null)
					throw new ArgumentException(
						"Для использования фильтра в журнале, view журнала должна была получить IGtkViewResolver для создания view", nameof(viewResolver));
				FilterView = viewResolver.Resolve(filterViewModel);
				if(FilterView == null)
					throw new InvalidOperationException($"Не найдена View для {filterViewModel.GetType()}");
				hboxFilter.Add(FilterView);
				FilterView.Show();
				checkShowFilter.Visible = true;
				checkShowFilter.Active = hboxFilter.Visible = ViewModel.IsFilterShow;
			}
			else {
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
			}

			if(ViewModel.SearchEnabled) {
				if(viewResolver == null)
					throw new ArgumentException(
						"Для использования поиска в журнале, view журнала должна была получить IGtkViewResolver для создания view", nameof(viewResolver));
				Widget searchView = viewResolver?.Resolve((ViewModelBase)ViewModel.Search);
				hboxSearch.Add(searchView);
				searchView.Show();
			}

			tableview.ColumnsConfig = TreeViewColumnsConfigFactory.Resolve(ViewModel);
			GtkScrolledWindow.Vadjustment.ValueChanged += Vadjustment_ValueChanged;

			SetItemsSource();

			ViewModel.Refresh();
			UpdateButtonActions();
			SetTotalLableText();
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.FooterInfo))
				Application.Invoke((s, args) => labelFooter.Markup = ViewModel.FooterInfo);
			if(e.PropertyName == nameof(ViewModel.TableSelectionMode))
				SetSelectionMode(ViewModel.TableSelectionMode);
			if(e.PropertyName == nameof(ViewModel.IsFilterShow))
				checkShowFilter.Active = hboxFilter.Visible = ViewModel.IsFilterShow;
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
				//К сожалению этот способ определения неподдерживаемых символов на винде для не цветных спинеров все равно возвращает 0, даже если символ не поддерживается.
				if(layout.UnknownGlyphsCount == 0) {
					CountingTextSpinner = new TextSpinner(spinner);
					break;
				}
				logger.Debug($"Спинер {spinner.GetType()} пропущен, так как используемый шрифт не поддеживает {layout.UnknownGlyphsCount} из {new StringInfo(allChars).LengthInTextElements} используемых символов.");
			}
		}

		void SetSelectionMode(JournalSelectionMode mode)
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

				SetItemsSource();

				if(!ViewModel.DataLoader.FirstPage || ViewModel.DataLoader.ItemsCountForNextLoad != null) {
					isRedrawInProgress = true;
					GtkHelper.WaitRedraw();
					if(GtkScrolledWindow?.Vadjustment != null) {
						GtkScrolledWindow.Vadjustment.Value = lastScrollPosition;
					}
					isRedrawInProgress = false;
				}

				if(ViewModel.ExpandAfterReloading) {
					tableview.ExpandAll();
				}

				foreach(var column in tableview.Columns) {
					column.QueueResize();
				}
			});
		}

		private void SetItemsSource() {
			if(tableview.ColumnsConfig.TreeModelFunc != null) {
				tableview.YTreeModel = tableview.ColumnsConfig.TreeModelFunc.Invoke();
			}
			else {
				tableview.ItemsDataSource = ViewModel.Items;
			}
		}

		private LoadingState loadingState;
		private uint _showProgressTimer;
		private uint _pulseProgressTimer;

		void DataLoader_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			if(loadingState != LoadingState.InProgress && e.LoadingState == LoadingState.InProgress) {
				_showProgressTimer = GLib.Timeout.Add(ShowProgressbarDelay, new GLib.TimeoutHandler(StartLoadProgress));
			}

			loadingState = e.LoadingState;
		}

		bool StartLoadProgress()
		{
			progressbarLoading.Visible = loadingState == LoadingState.InProgress;

			if(loadingState == LoadingState.InProgress) {
				_pulseProgressTimer = GLib.Timeout.Add(ProgressPulseTime, new GLib.TimeoutHandler(PulseProgress));
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
			if(!ViewModel.DataLoader.DynamicLoadingEnabled || GtkScrolledWindow.Vadjustment.Value + GtkScrolledWindow.Vadjustment.PageSize < GtkScrolledWindow.Vadjustment.Upper ||  isRedrawInProgress)
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

		private readonly List<Action<object[]>> buttonRefreshActions = new List<Action<object[]>>();

		private void ConfigureActions()
		{
			// Пробуем использовать новую модель действий
			if(ViewModel.ActionsViewModel is IButtonJournalActionsViewModel buttonActionsViewModel) {
				buttonActionsView = new ButtonJournalActionsView(buttonActionsViewModel);
				hboxButtons.PackStart(buttonActionsView, true, true, 0);
				buttonActionsView.ShowAll();
			}
			// Обратная совместимость - старый способ через NodeActions
			else {
				buttonRefreshActions.Clear();
				
				foreach(var action in ViewModel.NodeActions) {
					Button actionWidget;
					var actionForClosure = action;
					
					if(action.ChildActions.Any()) {
						MenuButton menuButton = new MenuButton();
						menuButton.Label = action.GetTitle(new object[] { });
						Menu childActionButtons = new Menu();
						foreach(var childAction in action.ChildActions) {
							childActionButtons.Add(CreateMenuItemWidget(childAction));
						}
						menuButton.Menu = childActionButtons;
						actionWidget = menuButton;
						if (action.ExecuteAction == null)
							action.ExecuteAction = items => menuButton.Press();
					} else {
						var button = new Button();
						button.Label = action.GetTitle(new object[] { });
						button.Clicked += (sender, args) =>  ExecuteAction(action);
						actionWidget = button;
					}
					
					buttonRefreshActions.Add((s) => actionWidget.Label = actionForClosure.GetTitle(s));
					buttonRefreshActions.Add((s) => actionWidget.Sensitive = actionForClosure.GetSensitivity(s));
					buttonRefreshActions.Add((s) => actionWidget.Visible = actionForClosure.GetVisibility(s));

					actionWidget.ShowAll();

					hboxButtons.Add(actionWidget);
					Box.BoxChild addDocumentButtonBox = (Box.BoxChild)hboxButtons[actionWidget];
					addDocumentButtonBox.Expand = false;
					addDocumentButtonBox.Fill = false;
				}
			}

			tableview.RowActivated += OnTableViewRowActivated;
		}

		private void OnTableViewRowActivated(object o, RowActivatedArgs args)
		{
			var selectedItems = GetSelectedItems();
			
			// Пробуем использовать новую систему действий
			if(ViewModel.ActionsViewModel != null) {
				// Получаем информацию о ячейке, если возможно
				object node = selectedItems.Length > 0 ? selectedItems[0] : null;
				ViewModel.ActionsViewModel.OnCellDoubleClick(node, null, null);
				lastScrollPosition = GtkScrolledWindow.Vadjustment?.Value ?? 0;
			}
			// Обратная совместимость - старая система действий
			else if(ViewModel.RowActivatedAction != null)
			{
				if(ViewModel.RowActivatedAction.GetSensitivity(selectedItems))
				{
					ViewModel.RowActivatedAction.ExecuteAction(selectedItems);
					lastScrollPosition = GtkScrolledWindow.Vadjustment?.Value ?? 0;
				}
			}
			else
			{
				ExpandCollapseOnRowActivated(args);
			}
		}
		
		private void ExecuteAction(IJournalAction action)
		{
			var selectedItems = GetSelectedItems();
			if(action.GetSensitivity(selectedItems) && action.GetVisibility(selectedItems)) {
				lastScrollPosition = GtkScrolledWindow.Vadjustment?.Value ?? 0;
				action.ExecuteAction.Invoke(selectedItems);
			}
		}

		private void ExpandCollapseOnRowActivated(RowActivatedArgs args) {
			if(tableview.GetRowExpanded(args.Path)) {
				tableview.CollapseRow(args.Path);
			}
			else {
				tableview.ExpandRow(args.Path, false);
			}
		}

		private MenuItem CreateMenuItemWidget(IJournalAction action)
		{
			var menuItem = new MenuItem(action.GetTitle(new object[] { }));

			buttonRefreshActions.Add((s) => ((Label)menuItem.Child).LabelProp = action.GetTitle(s));
			buttonRefreshActions.Add((s) => menuItem.Sensitive = action.GetSensitivity(s));
			buttonRefreshActions.Add((s) => menuItem.Visible = action.GetVisibility(s));
			
			if(action.ChildActions.Any()) {
				var subMenu = new Menu();
				menuItem.Submenu = subMenu;
				foreach(var childAction in action.ChildActions) {
					subMenu.Add(CreateMenuItemWidget(childAction));
				}
			}
			else {
				menuItem.ButtonPressEvent += (o, args) => {
					if(args.Event.Button != (uint)GtkMouseButton.Left) return;
					ExecuteAction(action);
				};
			}

			return menuItem;
		}

		void Tableview_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			var selectedItems = GetSelectedItems();
			if(args.Event.Button != (uint)GtkMouseButton.Right || !ViewModel.PopupActions.Any(x => x.GetVisibility(selectedItems))) 
				return;
			
			var popupMenu = new Menu();
			foreach(var popupAction in ViewModel.PopupActions) {
				CreatePopupMenuActionsRecursively(popupMenu, popupAction, selectedItems);
			}

			popupMenu.ShowAll();
			popupMenu.Popup();
		}

		private void CreatePopupMenuActionsRecursively(Menu menu, IJournalAction popupAction, object[] selectedItems)
		{
			if (!popupAction.GetVisibility(selectedItems))
				return; // Если элемент не виден, то не хуй его создавать.
			var item = new MenuItem(popupAction.GetTitle(selectedItems));
			item.Sensitive = popupAction.GetSensitivity(selectedItems);
			
			if(popupAction.ChildActions.Any()) {
				var subMenu = new Menu();
				foreach(var childAction in popupAction.ChildActions)
					CreatePopupMenuActionsRecursively(subMenu, childAction, selectedItems);
				
				item.Submenu = subMenu;
			}
			//Действия выполняются только для самых последних дочерних JournalActions
			else {
				item.ButtonPressEvent += (o, args) => {
					if(args.Event.Button != (uint)GtkMouseButton.Left) return;
					ExecuteAction(popupAction);
				};
			}
			menu.Add(item);
		}

		void Selection_Changed(object sender, EventArgs e) {
			UpdateButtonActions();
		}

		private void UpdateButtonActions() {
			var selectedItems = GetSelectedItems();
			
			// Новая модель действий
			if(ViewModel.ActionsViewModel != null) {
				ViewModel.ActionsViewModel.OnSelectionChanged(selectedItems);
			}
			
			// Старый способ для обратной совместимости
			foreach(var item in buttonRefreshActions)
				item.Invoke(selectedItems);
		}

		private bool isDestroyed = false;

		public override void Destroy()
		{
			isDestroyed = true;
			
			ViewModel.DataLoader.ItemsListUpdated -= ViewModel_ItemsListUpdated;
			ViewModel.DataLoader.LoadingStateChanged -= DataLoader_LoadingStateChanged;
			ViewModel.DataLoader.TotalCountChanged -= DataLoader_TotalCountChanged;
			if(ThrowExceptionOnDataLoad)
				ViewModel.DataLoader.LoadError -= DataLoader_LoadError;
			
			GLib.Source.Remove(_showProgressTimer);
			GLib.Source.Remove(_pulseProgressTimer);
			
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			
			ViewModel.DataLoader.CancelLoading();
			
			tableview?.Destroy();

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
			// Пробуем использовать новую модель действий
			if(ViewModel.ActionsViewModel != null) {
				ViewModel.ActionsViewModel.OnKeyPressed(hotKey);
				return;
			}
			
			// Старый способ для обратной совместимости
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

	public enum JournalActionsType {
		ButtonActions,
		PopupActions
	}
}
