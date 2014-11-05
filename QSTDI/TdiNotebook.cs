using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gtk;
using NLog;

namespace QSTDI
{
	[System.ComponentModel.ToolboxItem(true)]
	public class TdiNotebook : Gtk.Notebook, ITdiTabParent
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ReadOnlyCollection<TdiTabInfo> Tabs;
		public bool UseSliderTab = true;
		private List<TdiTabInfo> _tabs;

		public event EventHandler<TdiOpenObjDialogEventArgs> CreateDialogWidget;

		public TdiNotebook()
		{
			_tabs = new List<TdiTabInfo>();
			Tabs = new ReadOnlyCollection<TdiTabInfo>(_tabs);
			this.ShowTabs = true;
		}

		public bool CloseAllTabs()
		{
			if (_tabs.Exists (t => (t.MasterTab is ITdiDialog && (t.MasterTab as ITdiDialog).HasChanges) ||
				(t.MasterTab is TdiSliderTab && (t.MasterTab as TdiSliderTab).ActiveDialog != null && (t.MasterTab as TdiSliderTab).ActiveDialog.HasChanges))) {
				string Message = "Вы действительно хотите закрыть программу? Все несохраненные изменения будут утеряны.";
				MessageDialog md = new MessageDialog ((Window)this.Toplevel, DialogFlags.Modal,
					                   MessageType.Question, 
					                   ButtonsType.YesNo,
					                   Message);
				int result = md.Run ();
				md.Destroy ();
				if (result == (int)ResponseType.No) 
					return false;
			} 

			while(_tabs.Count > 0)
			{
				CloseTab (_tabs[0].MasterTab);
			}

			return true;
		}

		public void AddTab(ITdiTab tab, int after = -1)
		{
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
			if(UseSliderTab && tab is ITdiJournal)
			{
				TdiSliderTab slider = new TdiSliderTab((ITdiJournal)tab);
				tab = slider;
			}
			tab.CloseTab += HandleCloseTab;
			tab.TabNameChanged += OnTabNameChanged;
			int inserted;
			if (after >= 0)
				inserted = this.InsertPage((Widget)tab, box, after + 1);
			else
				inserted = this.AppendPage((Widget)tab, box);
			tab.TabParent = this;
			(tab as Widget).Show();
			this.ShowTabs = true;
			_tabs.Add(new TdiTabInfo(tab, nameLable));
			this.CurrentPage = inserted;
		}

		void OnTabNameChanged (object sender, TdiTabNameChangedEventArgs e)
		{
			ITdiTab tab = sender as ITdiTab;
			TdiTabInfo info = _tabs.Find(i => i.MasterTab == tab);
			if (_tabs.Exists(i => i.SlaveTabs.Contains(tab)))
				info.TabNameLabel.LabelProp = ">" + e.NewName;
			else
				info.TabNameLabel.LabelProp = e.NewName;
		}

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab, bool CanSlided = true)
		{
			TdiTabInfo info = _tabs.Find(t => t.MasterTab == masterTab);
			if (info == null)
				throw new NullReferenceException("Мастер вкладка не найдена в списке активных вкладок.");

			if(UseSliderTab && slaveTab is ITdiJournal)
			{
				TdiSliderTab slider = new TdiSliderTab((ITdiJournal)slaveTab);
				slaveTab = slider;
			}

			info.SlaveTabs.Add(slaveTab);
			this.AddTab(slaveTab, masterTab);
			OnTabNameChanged(slaveTab, new TdiTabNameChangedEventArgs(slaveTab.TabName));
		}

		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool CanSlided = true)
		{
			AddTab(tab, this.PageNum(afterTab as Widget));
		}

		void HandleCloseTab (object sender, TdiTabCloseEventArgs e)
		{
			if(!e.AskSave || SaveIfNeed((ITdiTab)sender))
				CloseTab((ITdiTab)sender);
		}

		void OnCloseButtonClicked (object sender, EventArgs e)
		{
			Widget boxWidget = (sender as Widget).Parent;
			ITdiTab tab = null;
			foreach(Widget wid in this.Children)
			{
				if(this.GetTabLabel(wid) == boxWidget)
					tab = (ITdiTab)wid;
			}

			if(tab == null)
			{
				logger.Warn("Не найден вкладка соответствующая кнопке закрыть.");
				return;
			}

			if(SaveIfNeed(tab))
				CloseTab(tab);
		}

		private bool SaveIfNeed(ITdiTab tab)
		{
			ITdiDialog dlg;

			TdiTabInfo info = _tabs.Find(i => i.MasterTab == tab);
			if(info.SlaveTabs.Count > 0)
			{
				string Message = "Сначала надо закрыть подчиненую вкладку.";
				MessageDialog md = new MessageDialog ( (Window)this.Toplevel, DialogFlags.Modal,
					MessageType.Warning, 
					ButtonsType.Ok,
					Message);
				md.Run ();
				md.Destroy();
				this.CurrentPage = this.PageNum(info.SlaveTabs[0] as Widget);
				return false;
			}

			if (tab is ITdiDialog)
				dlg = tab as ITdiDialog;
			else if (tab is TdiSliderTab && (tab as TdiSliderTab).ActiveDialog != null)
				dlg = (tab as TdiSliderTab).ActiveDialog;
			else
				return true;

			if(dlg.HasChanges)
			{
				string Message = "На вкладке есть изменения. Сохранить изменения перед закрытием?";
				MessageDialog md = new MessageDialog ( (Window)this.Toplevel, DialogFlags.Modal,
					MessageType.Question, 
					ButtonsType.YesNo,
					Message);
				int result = md.Run ();
				md.Destroy();
				if(result == (int)ResponseType.Yes)
				{
					if(!dlg.Save() )
					{
						logger.Warn("Вкладка не сохранена. Отмена закрытия...");
						return false;
					}
				}
			}
			return true;
		}

		public ITdiDialog OnCreateDialogWidget(TdiOpenObjDialogEventArgs eventArgs)
		{
			if (CreateDialogWidget != null)
			{
				CreateDialogWidget(this, eventArgs);
				return eventArgs.ResultDialogWidget;
			}
			else
			{
				logger.Warn("Нет подписчика на событие создания диалога. Диалог не создан.");
				return null;
			}
		}

		private void CloseTab(ITdiTab tab)
		{
			//Закрываем вкладку
			bool IsCurrent = this.CurrentPageWidget == tab as Widget;
			_tabs.RemoveAll(t => t.MasterTab == tab);
			_tabs.ForEach(t => t.SlaveTabs.RemoveAll(s => s == tab));
			if (IsCurrent)
				this.PrevPage();
			this.Remove((Widget)tab);
			(tab as Widget).Destroy();
			logger.Debug("Вкладка удалена");
		}

		public TdiBeforeCreateResultFlag BeforeCreateNewTab(object subject, ITdiTab masterTab, bool CanSlided = true)
		{
			return TdiBeforeCreateResultFlag.Ok;
		}

		public TdiBeforeCreateResultFlag BeforeCreateNewTab(System.Type subjectType, ITdiTab masterTab, bool CanSlided = true)
		{
			if (subjectType == null) //Потому что при null, может вызваться эта функция.
				BeforeCreateNewTab ((object)null, masterTab, CanSlided);
			return TdiBeforeCreateResultFlag.Ok;
		}
	}

	public class TdiTabInfo
	{
		public ITdiTab MasterTab;
		public Label TabNameLabel;
		public List<ITdiTab> SlaveTabs;

		public TdiTabInfo(ITdiTab master, Label label)
		{
			MasterTab = master;
			TabNameLabel = label;
			SlaveTabs = new List<ITdiTab>();
		}
	}
}

