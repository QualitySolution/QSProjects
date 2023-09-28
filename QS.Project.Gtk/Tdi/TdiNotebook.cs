using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Utilities.Text;
using QS.Utilities;
using QS.ViewModels.Extension;
using QS.Journal.GtkUI;

namespace QS.Tdi.Gtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public class TdiNotebook : Notebook, ITdiTabParent
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<TabAddedEventArgs> TabAdded;
		public event EventHandler<TabSwitchedEventArgs> TabSwitched;
		public event EventHandler<TabClosedEventArgs> TabClosed;

		public ReadOnlyCollection<TdiTabInfo> Tabs;
		public bool DefaultUseSlider = true;

		private List<TdiTabInfo> _tabs;
		private Menu _tabTitlePopUpMenu;
		private MenuItem _closeMenuItem;
		private MenuItem _closeOtherMenuItem;
		private MenuItem _closeRightMenuItem;
		private MenuItem _closeLeftMenuItem;
		private EventBox _tabTitleEventBoxClickedOn = null;

        private int _currentColor;
        private bool _useTabColors;
		public bool UseTabColors
        {
            get => _useTabColors;
            set 
            {
                _useTabColors = value;
                if (value)
                {
                    _currentColor = Colors?.Length - 1 ?? 0;
                }
            }
        }
        private bool _keepColors;
        public bool KeepColors
        {
            get => _keepColors;
            set
            {// сохранение у вкладок своего цвета, возможны повторения если true
                _keepColors = value;
                if (!value && _useTabColors)
                {
                    this.PageReordered += RecalculateColors;
                    this.TabClosed += RecalculateColors;
                }
            }
        }
		public string[] Colors { get; set; }
        public string Markup;
        public bool AllowToReorderTabs { get; set; } = true;
        public static int MaxTabNameLenght = 100;

		#region Внешние зависимости
		public ITDIWidgetResolver WidgetResolver { get; set; } = new DefaultTDIWidgetResolver();
		#endregion

		public TdiNotebook()
		{
			_tabs = new List<TdiTabInfo>();
			Tabs = new ReadOnlyCollection<TdiTabInfo>(_tabs);
			this.ShowTabs = true;
			WidgetEvent += TdiNotebook_WidgetEvent;

			_tabTitlePopUpMenu = new Menu();

			_closeMenuItem = new MenuItem("Закрыть");
			_closeMenuItem.ButtonReleaseEvent += OnCloseMenuItemButtonReleased;

			_closeOtherMenuItem = new MenuItem("Закрыть остальные");
			_closeOtherMenuItem.ButtonReleaseEvent += OnCloseOtherMenuItemButtonReleased;

			_closeRightMenuItem = new MenuItem("Закрыть вкладки справа");
			_closeRightMenuItem.ButtonReleaseEvent += OnCloseRightMenuItemButtonReleased;

			_closeLeftMenuItem = new MenuItem("Закрыть вкладки слева");
			_closeLeftMenuItem.ButtonReleaseEvent += OnCloseLeftMenuItemButtonReleased;

			_tabTitlePopUpMenu.Add(_closeMenuItem);
			_tabTitlePopUpMenu.Add(_closeOtherMenuItem);
			_tabTitlePopUpMenu.Add(_closeRightMenuItem);
			_tabTitlePopUpMenu.Add(_closeLeftMenuItem);

			_tabTitlePopUpMenu.ShowAll();
		}

		private void OnCloseMenuItemButtonReleased(object o, ButtonReleaseEventArgs args) {
			if(args.Event.Button != (uint)GtkMouseButton.Left
				|| _tabTitleEventBoxClickedOn is null
				|| o != _closeMenuItem)
				return;

			var currentTab = Tabs.FirstOrDefault(x => x.TabNameLabel.Parent == _tabTitleEventBoxClickedOn);

			if(currentTab != null) {
				AskToCloseTab(currentTab.TdiTab);
			}

			_tabTitleEventBoxClickedOn = null;
		}

		private void OnCloseOtherMenuItemButtonReleased(object o, ButtonReleaseEventArgs args) {
			if(args.Event.Button != (uint)GtkMouseButton.Left
				|| o != _closeOtherMenuItem)
				return;

			var currentTab = Tabs.FirstOrDefault(x => x.TabNameLabel.Parent == _tabTitleEventBoxClickedOn);

			var excludedNearTabs = GetParentAndHigher(currentTab.TdiTab);

			excludedNearTabs.Add(currentTab.TdiTab);

			if(currentTab != null) {
				var tabsToClose = Tabs
					.Where(x => !excludedNearTabs.Contains(x.TdiTab))
					.ToList();

				while(tabsToClose.Count > 0) {
					var firstTab = tabsToClose.First();

					if(!AskToCloseTabWithSlaves(firstTab.TdiTab)) {
						return;
					}

					tabsToClose = Tabs
						.Where(x => !excludedNearTabs.Contains(x.TdiTab))
						.ToList();
				}
			}

			_tabTitleEventBoxClickedOn = null;
		}

		private void OnCloseRightMenuItemButtonReleased(object o, ButtonReleaseEventArgs args) {
			if(args.Event.Button != (uint)GtkMouseButton.Left
				|| o != _closeRightMenuItem)
				return;

			var currentTab = Tabs.FirstOrDefault(x => x.TabNameLabel.Parent == _tabTitleEventBoxClickedOn);

			if(currentTab != null) {
				var currentTabIndex = Tabs.IndexOf(currentTab);

				var tabsToClose = Tabs.Skip(currentTabIndex + 1).ToList();

				while(tabsToClose.Count > 0) {
					var firstTab = tabsToClose.First();

					if(!AskToCloseTab(firstTab.TdiTab)) {
						return;
					}

					tabsToClose.RemoveAt(0);
				}
			}

			_tabTitleEventBoxClickedOn = null;
		}

		private void OnCloseLeftMenuItemButtonReleased(object o, ButtonReleaseEventArgs args) {
			if(args.Event.Button != (uint)GtkMouseButton.Left
				|| o != _closeLeftMenuItem)
				return;

			var currentTab = Tabs.FirstOrDefault(x => x.TabNameLabel.Parent == _tabTitleEventBoxClickedOn);

			var excludedNearTabs = GetParentAndHigher(currentTab.TdiTab);
			excludedNearTabs.Add(currentTab.TdiTab);

			if(currentTab != null) {
				var currentTabIndex = Tabs.IndexOf(currentTab);

				var tabsToClose = Tabs
					.Take(currentTabIndex)
					.Where(x => !excludedNearTabs.Contains(x.TdiTab))
					.ToList();

				while(tabsToClose.Count > 0) {
					var firstTab = tabsToClose.First();

					if(!AskToCloseTabWithSlaves(firstTab.TdiTab)) {
						return;
					}

					tabsToClose = Tabs
						.Take(currentTabIndex)
						.Where(x => !excludedNearTabs.Contains(x.TdiTab))
						.ToList();
				}

				currentTabIndex = Tabs.IndexOf(currentTab);
			}

			_tabTitleEventBoxClickedOn = null;
		}

		private void TdiNotebook_WidgetEvent(object o, WidgetEventArgs args) {
			//Блокируем событие клика если нет вкладок, из-за того что приложение
			//крашиться если происходит множество кликов закрытия в очень короткий промежуток времени.
			if(args.Event.Type == Gdk.EventType.ButtonRelease && NPages == 0) {
				args.RetVal = true;
			}
		}

		protected override bool OnWidgetEvent(Gdk.Event evnt)
		{
			//Блокируем событие клика если нет вкладок, из-за того что приложение
			//крашиться если происходит множество кликов закрытия в очень короткий промежуток времени.
			if (evnt.Type == Gdk.EventType.ButtonRelease && NPages == 0)
				return true;
			return base.OnWidgetEvent(evnt);
		}

		void OnTabNameChanged(object sender, TdiTabNameChangedEventArgs e)
		{
			ITdiTab tab = sender as ITdiTab;
			TdiTabInfo info = _tabs.Find(i => i.TdiTab == tab);
			if(info == null) {
				logger.Warn("Не найдена вкладка");
				return;
			}
			TdiTabInfo masterTabInfo = _tabs.Find(i => i.SlaveTabs.Contains(tab));
			var newName = StringManipulationHelper.EllipsizeMiddle(e.NewName, MaxTabNameLenght);
			if(masterTabInfo != null) {
				info.TabNameLabel.Markup = _useTabColors ? String.Format(Markup, masterTabInfo.Color, newName) : ">" + newName;
				info.TabNameLabel.TooltipText =  e.NewName?.Length > MaxTabNameLenght ? e.NewName + "\n" : String.Empty 
					+ String.Format("Открыто из {0}", masterTabInfo.TdiTab.TabName);
			}
			else {
				info.TabNameLabel.Markup = _useTabColors ? String.Format(Markup, info.Color, newName) : newName;
				info.TabNameLabel.TooltipText = e.NewName?.Length > MaxTabNameLenght ? e.NewName + "\n" : null;
			}
		}

		public ITdiTab FindTab(string hashName, string masterHashName = null)
		{
			if(String.IsNullOrWhiteSpace(hashName))
				return null;

			TdiTabInfo tab = null;
			if(String.IsNullOrWhiteSpace(masterHashName)) {
				tab = Tabs.FirstOrDefault(x => x.MasterTabInfo == null && x.TdiTab.CompareHashName(hashName));
				return tab?.TdiTab;
			} else {
				var master = Tabs.FirstOrDefault(x => x.TdiTab.CompareHashName(masterHashName));
				if(master == null)
					return null;
				return master.SlaveTabs.FirstOrDefault(x => x.CompareHashName(hashName));
			}
		}

		public void SwitchOnTab(ITdiTab tab)
		{
			var widget = GetTabBoxForTab(tab);
			if(widget == null)
				return;
			this.CurrentPage = this.PageNum(widget);
		}

		public ITdiTab CurrentTab => CurrentPage >= 0 ? Tabs[CurrentPage].TdiTab : null;

		#region Открытие вкладки

		public void AddTab(ITdiTab tab, int after = -1)
		{
			if(tab.FailInitialize) {
				logger.Warn("Вкладка <{0}> не добавлена, так как сообщает что построена с ошибкой(Свойство FailInitialize) .",
					tab.TabName
				);
				return;
			}
			HBox box = new HBox();
			EventBox eventBox = new EventBox();
			eventBox.ButtonReleaseEvent += OnTitleEventBoxButtonReleased;

			Label nameLabel = new Label();
			if (after == -1 && _useTabColors)
			{// открыли не подчиненную вкладку - поменяли цвет
			    SwitchCurrentColor();
			}

			var tabName = StringManipulationHelper.EllipsizeMiddle(tab.TabName, MaxTabNameLenght);
			if(tab.TabName?.Length > MaxTabNameLenght)
				nameLabel.TooltipText = tab.TabName;
			
			nameLabel.Markup = _useTabColors ? String.Format(Markup, Colors[_currentColor], tabName) : tabName;
			eventBox.Add(nameLabel);
			box.Add(eventBox);
			Image closeImage = new Image(Stock.Close, IconSize.Menu);
			Button closeButton = new Button(closeImage);
			closeButton.Relief = ReliefStyle.None;
			closeButton.Clicked += OnCloseButtonClicked;
			closeButton.ModifierStyle.Xthickness = 0;
			closeButton.ModifierStyle.Ythickness = 0;
			box.Add(closeButton);
			box.ShowAll();
			var journalTab = tab as ITdiJournal;
			if(journalTab != null && ((journalTab.UseSlider == null && DefaultUseSlider) || journalTab.UseSlider.Value)) {
				TdiSliderTab slider = new TdiSliderTab((ITdiJournal)tab, WidgetResolver);
				tab = slider;
			}
			tab.TabNameChanged += OnTabNameChanged;
			_tabs.Add(new TdiTabInfo(tab, nameLabel) { Color = _useTabColors ? Colors[_currentColor] : null });
			var vbox = new TabVBox(tab, WidgetResolver);
			int inserted;
			if(after >= 0)
				inserted = this.InsertPage(vbox, box, after + 1);
			else
				inserted = this.AppendPage(vbox, box);

			this.SetTabReorderable(vbox, AllowToReorderTabs);
			tab.TabParent = this;
			vbox.Show();
			this.ShowTabs = true;
			if(TabAdded != null)
				TabAdded(this, new TabAddedEventArgs(tab));
			this.CurrentPage = inserted;
			logger.Debug("Добавлена вкладка '{0}'", tab.TabName);

			//I-867 Если вкладка "заказы",
			if(tab is ITdiTabAddedNotifier) {
				//то открыть окно "контрагенты"
				((ITdiTabAddedNotifier)tab).OnTabAdded();
			}
		}

		private void OnTitleEventBoxButtonReleased(object o, ButtonReleaseEventArgs args) {
			_tabTitleEventBoxClickedOn = (o as EventBox);

			if(args.Event.Button == (uint)GtkMouseButton.Middle) {
				var currentTab = Tabs.FirstOrDefault(x => x.TabNameLabel.Parent == _tabTitleEventBoxClickedOn);

				AskToCloseTab(currentTab.TdiTab);

				return;
			}

			if(args.Event.Button != (uint)GtkMouseButton.Right)
				return;

			_tabTitlePopUpMenu.Popup();
		}

		protected override void OnPageReordered(Widget p0, uint p1)
        {
			var tab = ((TabVBox)p0).Tab;

			var slaves = GetSlaveTabs(tab);

			if (p1 > 0 && GetTabReorderable(p0))
			{
				TabVBox vboxBefore = (TabVBox)GetNthPage((int)p1 - 1);

				if (GetSlaveTabs(vboxBefore.Tab).Any())
				{
					ReorderChild(p0, (int)p1 + 1);
					return;
				}
			}

			foreach (var slave in slaves)
			{
				var newPosition = (int)p1;
                if (PageNum(GetTabBoxForTab(slave)) > newPosition)
                {
                    newPosition++;
                }

                ReorderChild(GetTabBoxForTab(slave), newPosition);
			}
			base.OnPageReordered(p0, p1);
        }

        #region Работа с цветными префиксами

        private void RecalculateColors(object o, EventArgs args)
        {
            _currentColor = Colors.Length - 1;// цвета всегда будут начинаться с Colors[0]
            for (var i = 0; i < NPages; i++)
            {// массив _tabs не пересортировывается при смене порядка, так что ищем по страницам
                var master = _tabs.FirstOrDefault(tinfo => tinfo.TdiTab == ((TabVBox)GetNthPage(i)).Tab);
                if (master?.MasterTabInfo == null)
                {// находим инфо родителей, меняем у них цвета и разметку
                    SwitchCurrentColor();
                    master.Color = Colors[_currentColor];
                    master.TabNameLabel.Markup = String.Format(Markup, master.Color, master.TdiTab.TabName);
                    foreach (var servant in _tabs.Where(t => t.MasterTabInfo != null && master == t.MasterTabInfo))
                    {// а затем меняем разметку в инфо лейбла у детей
                        servant.TabNameLabel.Markup = String.Format(Markup, master.Color, servant.TdiTab.TabName);
                    }
                }
            }
        }

        private void SwitchCurrentColor() =>
            _currentColor = _currentColor == (Colors.Length - 1) || Colors.Length == 1 ? 0 : _currentColor + 1;

        #endregion

		private IList<ITdiTab> GetSlaveTabs(ITdiTab tab)
		{
			return _tabs.Find(t => t.TdiTab == tab).SlaveTabs;
		}

		private ITdiTab GetParent(ITdiTab tab) {
			return _tabs
				.Where(t => t.SlaveTabs.Contains(tab))
				.Select(x => x.TdiTab)
				.FirstOrDefault();
		}

		private IList<ITdiTab> GetParentAndHigher(ITdiTab tab) {

			var result = new List<ITdiTab>();

			var currentParent = GetParent(tab);

			if(currentParent is null)
				return result;

			result.Add(currentParent);

			var higherParent = GetParent(currentParent);

			while (higherParent != null) {
				result.Add(higherParent);
				result.AddRange(GetSlaveTabs(higherParent).Where(x => !result.Contains(x)));
				higherParent = GetParent(higherParent);
			}

			return result;
		}

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab)
		{
			TdiTabInfo info = _tabs.Find(t => t.TdiTab == masterTab);
			if(info == null)
				throw new NullReferenceException("Мастер вкладка не найдена в списке активных вкладок.");

			var journalTab = slaveTab as ITdiJournal;
			if(journalTab != null && (!journalTab.UseSlider.HasValue && DefaultUseSlider || journalTab.UseSlider.Value)) {
				TdiSliderTab slider = new TdiSliderTab((ITdiJournal)slaveTab, WidgetResolver);
				slaveTab = slider;
			}

			info.SlaveTabs.Add(slaveTab);
			this.AddTab(slaveTab, masterTab);
			this.SetTabReorderable(GetTabBoxForTab(slaveTab), false);
			var addedTabInfo = _tabs.Find(t => t.TdiTab == slaveTab);
			addedTabInfo.MasterTabInfo = info;
			OnTabNameChanged(slaveTab, new TdiTabNameChangedEventArgs(slaveTab.TabName));
		}

		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool CanSlided = true)
		{
			if(afterTab == null)
				AddTab(tab);
			else {
				var tabBox = GetTabBoxForTab(afterTab);
				if(tabBox == null) {
					AddTab(tab);
				} else {
					AddTab(tab, this.PageNum(tabBox));
				}
			}
		}

		public ITdiTab OpenTab(Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, Type[] argTypes = null, object[] args = null)
		{
			ITdiTab tab = newTabFunc.Invoke();
			Type tabType = tab.GetType();
			string hashName = TabHashHelper.GetTabHash(tabType, argTypes ?? new Type[] { }, args ?? new object[] { });
			return OpenTab(hashName, () => tab, afterTab);
		}

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, bool canSlided = true)
		{
			ITdiTab tab = FindTab(hashName);

			if(tab == null) {
				if(afterTab == null)
					AddTab(newTabFunc());
				else
					AddTab(newTabFunc(), afterTab);
			} else {
				logger.Debug("Вкладка c хешом {0} уже открыта, переключаемся...", hashName);
				SwitchOnTab(tab);
			}

			return tab;
		}

		public ITdiTab OpenTab<TTab>(ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { }, new object[] { }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1>(TArg1 arg1, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1) }, new object[] { arg1 }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2>(TArg1 arg1, TArg2 arg2, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 }, afterTab);
		}

		public ITdiTab OpenTab<TTab, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ITdiTab afterTab = null) where TTab : ITdiTab
		{
			return TabHashHelper.OpenTabSelfCreateTab(this, typeof(TTab), new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1, arg2, arg3 }, afterTab);
		}

		#endregion

		internal void OnSliderTabAdded(object sender, ITdiTab tab)
		{
			if(TabAdded != null)
				TabAdded(sender, new TabAddedEventArgs(tab));
		}

		internal void OnSliderTabClosed(object sender, ITdiTab tab, CloseSource source)
		{
			TabClosed?.Invoke(sender, new TabClosedEventArgs(tab, source));
		}

		internal void OnSliderTabSwitched(object sender, ITdiTab tab)
		{
			if(TabSwitched != null)
				TabSwitched(this, new TabSwitchedEventArgs(tab));
		}

		protected override void OnSwitchPage(NotebookPage page, uint page_num)
		{
			var previous = (TabVBox)CurrentPageWidget;
			var previousTab = previous.Tab;
			base.OnSwitchPage(page, page_num);
			var currentTab = (this.CurrentPageWidget as TabVBox)?.Tab;
			currentTab = (currentTab as TdiSliderTab)?.ActiveDialog ?? currentTab;
			if(TabSwitched != null)
				TabSwitched(this, new TabSwitchedEventArgs(currentTab));
			if(previousTab.HandleSwitchOut != null)
				previousTab.HandleSwitchOut(currentTab);
			if(currentTab.HandleSwitchIn != null)
				currentTab.HandleSwitchIn(previousTab);
		}

		#region Закрытие вкладки

		public bool AskToCloseTabWithSlaves(ITdiTab tab, CloseSource source = CloseSource.External) {
			var slaves = GetSlaveTabs(tab).ToList();

			if(slaves.Count > 0) {
				while(slaves.Count > 0) {
					if(!AskToCloseTabWithSlaves(slaves.First())){
						return false;
					}
					slaves = GetSlaveTabs(tab).ToList();
				}
			}

			return AskToCloseTab(tab);
		}

		public bool AskToCloseTab(ITdiTab tab, CloseSource source = CloseSource.External)
		{
			var slider = tab.TabParent as TdiSliderTab;
			if(slider != null)
				return slider.AskToCloseTab(tab, source);

			if (CheckClosingSlaveTabs(tab))
				return false;
			
			ITDICloseControlTab cct = tab as ITDICloseControlTab;
			ITDICloseControlTab canCloseActiveDialog = (tab as TdiSliderTab)?.ActiveDialog as ITDICloseControlTab;
			if((cct != null && !cct.CanClose()) || (canCloseActiveDialog != null && !canCloseActiveDialog.CanClose())) {
				return false;
			}

			bool canClose = true;
			try
			{
				canClose = SaveIfNeed(tab);
			}
			catch (Exception e)
			{
				logger.Error(e, "Возникла ошибка при проверке закрытия вкладки");
			}
			if(canClose) {
				ForceCloseTab(tab, source);
				return true;
			}
			return false;
		}

		public void ForceCloseTab(ITdiTab tab, CloseSource source = CloseSource.External)
		{
			var slider = tab.TabParent as TdiSliderTab;
			if (slider != null) {
				slider.ForceCloseTab(tab, source);
				return;
			}

			TdiTabInfo info = _tabs.Find(i => i.TdiTab == tab);
			if(info == null) {
				logger.Warn("Вкладка предположительно уже закрыта, попускаем...");
				return;
			}

			while(info.SlaveTabs.Count > 0)
				ForceCloseTab(info.SlaveTabs[0], CloseSource.WithMasterPage);

			CloseTab(info, source);
		}

		void OnCloseButtonClicked(object sender, EventArgs e)
		{
			Widget boxWidget = (sender as Widget).Parent;
			TabVBox tab = null;
			foreach(Widget wid in this.Children) {
				if(this.GetTabLabel(wid) == boxWidget)
					tab = (TabVBox)wid;
			}

			if(tab == null) {
				logger.Warn("Не найден вкладка соответствующая кнопке закрыть.");
				return;
			}

			AskToCloseTab(tab.Tab, CloseSource.ClosePage);
		}


		/// <returns><c>true</c>, если можно закрывать вкладку, если закрытие вкладки отменено то <c>false</c></returns>
		private bool SaveIfNeed(ITdiTab tab)
		{
			if(CheckClosingSlaveTabs(tab))
				return false;

			ITdiDialog dlg;

			if(tab is ITdiDialog)
				dlg = tab as ITdiDialog;
			else
				dlg = (tab as TdiSliderTab)?.ActiveDialog as ITdiDialog;

			if(dlg == null)
				return true;

			var askSave = (dlg as IAskSaveOnCloseViewModel)?.AskSaveOnClose ?? true;

			if(askSave && dlg.HasChanges) {
				string Message = "На вкладке есть изменения. Сохранить изменения перед закрытием?";
				MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.Modal,
									   MessageType.Question,
									   ButtonsType.YesNo,
									   Message);
				int result = md.Run();
				md.Destroy();
				if(result == (int)ResponseType.Yes) {
					if(!dlg.Save()) {
						logger.Info("Вкладка не сохранена. Отмена закрытия...");
						return false;
					}
					else
						return true;
				}
				if(result == (int)ResponseType.No) {
					return true;
				}
				if(result == (int)ResponseType.DeleteEvent)
					return false;
			}
			return true;
		}

		public bool CheckClosingSlaveTabs(ITdiTab tab)
		{
			if (tab.TabParent is TdiSliderTab)
				tab = (ITdiTab)tab.TabParent;

			TdiTabInfo info = _tabs.Find(i => i.TdiTab == tab);
			if(info!= null && info.SlaveTabs.Count > 0)
			{
				logger.Warn($"У {tab.TabName} открыта подчиненная вкладка {info.SlaveTabs[0].TabName}");
				string Message = "Сначала надо закрыть подчиненную вкладку.";
				MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.Modal,
									   MessageType.Warning,
									   ButtonsType.Ok,
									   Message);
				md.Run();
				md.Destroy();
				this.CurrentPage = this.PageNum(GetTabBoxForTab(info.SlaveTabs[0]));
				return true;
			}

			return false;
		}

		private void CloseTab(TdiTabInfo info, CloseSource source)
		{
			if(info.SlaveTabs.Count > 0)
				throw new InvalidOperationException("Вкладка не может быть закрыта, если у нее есть подчинёные вкладки.");

			var tab = info.TdiTab;

			//Закрываем вкладку
			TabVBox tabBox = GetTabBoxForTab(tab);
			bool IsCurrent = this.CurrentPageWidget == tabBox;
			_tabs.RemoveAll(t => t.TdiTab == tab);
			_tabs.ForEach(t => t.SlaveTabs.RemoveAll(s => s == tab));
			if(IsCurrent)
				this.PrevPage();
			this.Remove(tabBox);
			var maybeSliderActiveDialog = (tab as TdiSliderTab)?.ActiveDialog;
			if(maybeSliderActiveDialog != null) {
				OnTabClosed(maybeSliderActiveDialog, CloseSource.WithParentPage);
			}
			OnTabClosed(tab, source);
			tab.OnTabClosed();
			if(tabBox != null && tabBox.Tab is Container) {
				GtkHelper.EnumerateAllChildren((Container)tabBox.Tab)
				.OfType<IMustBeDestroyed>().ToList()
				.ForEach(w => w.Destroy());
			}
			logger.Debug("Вкладка <{0}> удалена", tab.TabName);
			if(tabBox != null) {
				tabBox.Destroy();
			}

            tab.TabNameChanged -= OnTabNameChanged;
			
            if (tab is IDisposable)
            {
				(tab as IDisposable).Dispose();
				tab = null;
			}
			GC.Collect();
		}

		internal void OnTabClosed(ITdiTab tab, CloseSource source)
		{
			TabClosed?.Invoke(this, new TabClosedEventArgs(tab, source));
		}

		public bool CloseAllTabs()
		{
			if(_tabs.Exists(t => (t.TdiTab is ITdiDialog && (t.TdiTab as ITdiDialog).HasChanges) ||
			  (t.TdiTab is TdiSliderTab && ((t.TdiTab as TdiSliderTab)?.ActiveDialog as ITdiDialog)?.HasChanges == true))) {
				string Message = "Вы действительно хотите закрыть все вкладки? Все несохраненные изменения будут утеряны.";
				MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.Modal,
									   MessageType.Question,
									   ButtonsType.YesNo,
									   Message);
				(md.Image as Image).SetFromStock(Stock.Quit, IconSize.Dialog);
				int result = md.Run();
				md.Destroy();
				if(result == (int)ResponseType.No || result == (int)ResponseType.DeleteEvent)
					return false;
			}

			while(_tabs.Count > 0) {
				ForceCloseTab(_tabs[0].TdiTab, CloseSource.AppQuit);
			}

			return true;
		}

		#endregion

		#region Helpers
		private TabVBox GetTabBoxForTab(ITdiTab tab)
		{
			return this.Children.SingleOrDefault(w => {
				TabVBox tabBox = (w as TabVBox);
				if(tabBox.Tab == tab) {
					return true;
				}
				if(tabBox.Tab is TdiSliderTab) {
					return (tabBox.Tab as TdiSliderTab).Journal == tab;
				}
				return false;
			}) as TabVBox;
		}
		#endregion
	}

	public class TdiTabInfo
	{
		public TdiTabInfo MasterTabInfo;
		public ITdiTab TdiTab;
		public Label TabNameLabel;
		public List<ITdiTab> SlaveTabs = new List<ITdiTab>();
        public string Color;

		public TdiTabInfo(ITdiTab tab, Label label, TdiTabInfo masterTabInfo = null)
		{
			MasterTabInfo = masterTabInfo;
			TdiTab = tab;
			TabNameLabel = label;
		}
	}

	public class TabAddedEventArgs : EventArgs
	{
		private ITdiTab tab;
		public ITdiTab Tab {
			get { return tab; }
		}
		public TabAddedEventArgs(ITdiTab tab)
		{
			this.tab = tab;
		}
	}

	public class TabClosedEventArgs : EventArgs
	{
		private ITdiTab tab;

		public CloseSource CloseSource { get; private set; }

		public ITdiTab Tab {
			get { return tab; }
		}

		public TabClosedEventArgs(ITdiTab tab, CloseSource source)
		{
			this.tab = tab;
			CloseSource = source;
		}
	}

	public class TabSwitchedEventArgs : EventArgs
	{
		private ITdiTab tab;
		public ITdiTab Tab {
			get { return tab; }
		}
		public TabSwitchedEventArgs(ITdiTab tab)
		{
			this.tab = tab;
		}
	}
}
