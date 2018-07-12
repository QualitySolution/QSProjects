using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gtk;
using NLog;

namespace QSTDI
{
	[System.ComponentModel.ToolboxItem(true)]
	public class TdiNotebook : Gtk.Notebook, ITdiTabParent
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<TabAddedEventArgs> TabAdded;
		public event EventHandler<TabSwitchedEventArgs> TabSwitched;
		public event EventHandler<TabClosedEventArgs> TabClosed;

		public ReadOnlyCollection<TdiTabInfo> Tabs;
		public bool DefaultUseSlider = true;
		private List<TdiTabInfo> _tabs;

		public TdiNotebook()
		{
			_tabs = new List<TdiTabInfo>();
			Tabs = new ReadOnlyCollection<TdiTabInfo>(_tabs);
			this.ShowTabs = true;
		}

		public bool CloseAllTabs()
		{
			if(_tabs.Exists(t => (t.TdiTab is ITdiDialog && (t.TdiTab as ITdiDialog).HasChanges) ||
			  (t.TdiTab is TdiSliderTab && (t.TdiTab as TdiSliderTab).ActiveDialog != null && (t.TdiTab as TdiSliderTab).ActiveDialog.HasChanges))) {
				string Message = "Вы действительно хотите закрыть все вкладки? Все несохраненные изменения будут утеряны.";
				MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.Modal,
									   MessageType.Question,
									   ButtonsType.YesNo,
									   Message);
				(md.Image as Image).SetFromStock(Stock.Quit, IconSize.Dialog);
				int result = md.Run();
				md.Destroy();
				if(result == (int)ResponseType.No)
					return false;
			}

			while(_tabs.Count > 0) {
				while(_tabs[0].SlaveTabs.Count > 0)
					CloseTab(_tabs[0].SlaveTabs[0]);

				CloseTab(_tabs[0].TdiTab);
			}

			return true;
		}

		public void AddTab(ITdiTab tab, int after = -1)
		{
			if(tab.FailInitialize) {
				logger.Warn("Вкладка <{0}> не добавлена, так как сообщает что построена с ошибкой(Свойство FailInitialize) .",
					tab.TabName
				);
				return;
			}
			HBox box = new HBox();
			Label nameLable = new Label(tab.TabName);
			box.Add(nameLable);
			Gtk.Image closeImage = new Gtk.Image(Stock.Close, IconSize.Menu);
			Button closeButton = new Button(closeImage);
			closeButton.Relief = ReliefStyle.None;
			closeButton.Clicked += OnCloseButtonClicked;
			closeButton.ModifierStyle.Xthickness = 0;
			closeButton.ModifierStyle.Ythickness = 0;
			box.Add(closeButton);
			box.ShowAll();
			var journalTab = tab as ITdiJournal;
			if(journalTab != null && ((journalTab.UseSlider == null && DefaultUseSlider) || journalTab.UseSlider.Value)) {
				TdiSliderTab slider = new TdiSliderTab((ITdiJournal)tab);
				tab = slider;
			}
			tab.CloseTab += HandleCloseTab;
			tab.TabNameChanged += OnTabNameChanged;
			_tabs.Add(new TdiTabInfo(tab, nameLable));
			var vbox = new TabVBox(tab);
			int inserted;
			if(after >= 0)
				inserted = this.InsertPage(vbox, box, after + 1);
			else
				inserted = this.AppendPage(vbox, box);
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

		void OnTabNameChanged(object sender, TdiTabNameChangedEventArgs e)
		{
			ITdiTab tab = sender as ITdiTab;
			TdiTabInfo info = _tabs.Find(i => i.TdiTab == tab);
			if(info == null)
				throw new ApplicationException();
			TdiTabInfo masterTabInfo = _tabs.Find(i => i.SlaveTabs.Contains(tab));
			if(masterTabInfo != null) {
				info.TabNameLabel.LabelProp = ">" + e.NewName;
				info.TabNameLabel.TooltipText = String.Format("Открыто из {0}", masterTabInfo.TdiTab.TabName);
			} else
				info.TabNameLabel.LabelProp = e.NewName;
		}

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab)
		{
			TdiTabInfo info = _tabs.Find(t => t.TdiTab == masterTab);
			if(info == null)
				throw new NullReferenceException("Мастер вкладка не найдена в списке активных вкладок.");

			var journalTab = slaveTab as ITdiJournal;
			if(journalTab != null && ((journalTab.UseSlider == null && DefaultUseSlider) || journalTab.UseSlider.Value)) {
				TdiSliderTab slider = new TdiSliderTab((ITdiJournal)slaveTab);
				slaveTab = slider;
			}

			info.SlaveTabs.Add(slaveTab);
			this.AddTab(slaveTab, masterTab);
			var addedTabInfo = _tabs.Find(t => t.TdiTab == slaveTab);
			addedTabInfo.MasterTabInfo = info;
			OnTabNameChanged(slaveTab, new TdiTabNameChangedEventArgs(slaveTab.TabName));
		}

		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool CanSlided = true)
		{
			AddTab(tab, this.PageNum(GetTabBoxForTab(afterTab)));
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

		public ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null)
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

		internal void OnSliderTabAdded(object sender, ITdiTab tab)
		{
			if(TabAdded != null)
				TabAdded(sender, new TabAddedEventArgs(tab));
		}

		internal void OnSliderTabClosed(object sender, ITdiTab tab)
		{
			if(TabClosed != null)
				TabClosed(sender, new TabClosedEventArgs(tab));
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

		void HandleCloseTab(object sender, TdiTabCloseEventArgs e)
		{
			if(CheckClosingSlaveTabs((ITdiTab)sender))
				return;
			
			ITDICloseControlTab cct = sender as ITDICloseControlTab;
			if(cct != null && !cct.CanClose()) {
				return;
			}

			if(!e.AskSave || SaveIfNeed((ITdiTab)sender))
				CloseTab((ITdiTab)sender);
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

			if(CheckClosingSlaveTabs(tab.Tab))
				return;

			ITDICloseControlTab cct = tab.Tab as ITDICloseControlTab;
			if(cct != null && !cct.CanClose()) {
				return;
			}

			if(SaveIfNeed(tab.Tab))
				CloseTab(tab.Tab);
		}

		private bool SaveIfNeed(ITdiTab tab)
		{
			if(CheckClosingSlaveTabs(tab))
				return false;

			ITdiDialog dlg;

			if(tab is ITdiDialog)
				dlg = tab as ITdiDialog;
			else if(tab is TdiSliderTab && (tab as TdiSliderTab).ActiveDialog != null)
				dlg = (tab as TdiSliderTab).ActiveDialog;
			else
				return true;

			if(dlg.HasChanges) {
				string Message = "На вкладке есть изменения. Сохранить изменения перед закрытием?";
				MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.Modal,
									   MessageType.Question,
									   ButtonsType.YesNo,
									   Message);
				int result = md.Run();
				md.Destroy();
				if(result == (int)ResponseType.Yes) {
					if(!dlg.Save()) {
						logger.Warn("Вкладка не сохранена. Отмена закрытия...");
						return false;
					}
				}
				if(result == (int)ResponseType.No) {
					(dlg as Bin).Destroy();
					return true;
				}
				if(result == (int)ResponseType.DeleteEvent)
					return false;
			}
			return true;
		}

		public bool CheckClosingSlaveTabs(ITdiTab tab)
		{
			TdiTabInfo info = _tabs.Find(i => i.TdiTab == tab);
			if(info.SlaveTabs.Count > 0) {
				string Message = "Сначала надо закрыть подчиненую вкладку.";
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

		private void CloseTab(ITdiTab tab)
		{
			TdiTabInfo info = _tabs.Find(i => i.TdiTab == tab);
			if(info == null) {
				logger.Warn("Вкладка предположительно уже закрыта, попускаем...");
				return;
			}

			if(info.SlaveTabs.Count > 0) {
				throw new InvalidOperationException("Вкладка не может быть закрыта, если у нее есть подчинёные вкладки.");
			}

			//Закрываем вкладку
			TabVBox tabBox = GetTabBoxForTab(tab);
			bool IsCurrent = this.CurrentPageWidget == tabBox;
			_tabs.RemoveAll(t => t.TdiTab == tab);
			_tabs.ForEach(t => t.SlaveTabs.RemoveAll(s => s == tab));
			if(IsCurrent)
				this.PrevPage();
			this.Remove(tabBox);
			var maybeSliderActiveDialog = (tab as TdiSliderTab)?.ActiveDialog;
			if(maybeSliderActiveDialog != null)
				OnTabClosed(maybeSliderActiveDialog);
			OnTabClosed(tab);
			logger.Debug("Вкладка <{0}> удалена", tab.TabName);
			(tab as Widget).Destroy();
			tabBox.Destroy();
		}

		protected void OnTabClosed(ITdiTab tab)
		{
			if(TabClosed != null) {
				TabClosed(this, new TabClosedEventArgs(tab));
			}
		}

		private TabVBox GetTabBoxForTab(ITdiTab tab)
		{
			return this.Children.SingleOrDefault(w => (w as TabVBox).Tab == tab) as TabVBox;
		}
	}

	public class TdiTabInfo
	{
		public TdiTabInfo MasterTabInfo;
		public ITdiTab TdiTab;
		public Label TabNameLabel;
		public List<ITdiTab> SlaveTabs = new List<ITdiTab>();

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
		public ITdiTab Tab {
			get { return tab; }
		}
		public TabClosedEventArgs(ITdiTab tab)
		{
			this.tab = tab;
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

