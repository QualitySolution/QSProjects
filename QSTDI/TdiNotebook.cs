using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gtk;
using NLog;

namespace QSTDI
{
	[System.ComponentModel.ToolboxItem(true)]
	public class TdiNotebook : Gtk.Notebook
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ReadOnlyCollection<ITdiTab> Tabs;
		public bool UseSliderTab = true;
		private List<ITdiTab> _tabs;

		public event EventHandler<TdiOpenObjDialogEventArgs> CreateDialogWidget;

		public TdiNotebook()
		{
			_tabs = new List<ITdiTab>();
			Tabs = new ReadOnlyCollection<ITdiTab>(_tabs);
			this.ShowTabs = true;
		}

		public void AddTab(ITdiTab tab)
		{
			HBox box = new HBox();
			Label nameLable = new Label(tab.TabName);
			box.Add(nameLable);
			Gtk.Image closeImage = new Gtk.Image(Stock.Close, IconSize.Menu);
			Button closeButton = new Button(closeImage);
			closeButton.Relief = ReliefStyle.None;
			closeButton.Clicked += OnCloseButtonClicked;
			box.Add(closeButton);
			box.ShowAll();
			if(UseSliderTab && tab is ITdiJournal)
			{
				TdiSliderTab slider = new TdiSliderTab((ITdiJournal)tab);
				tab = slider;
			}
			tab.CloseTab += HandleCloseTab;
			this.AppendPage((Widget)tab, box);
			(tab as Widget).Show();
			this.ShowTabs = true;
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

		internal ITdiDialog OnCreateDialogWidget(TdiOpenObjDialogEventArgs eventArgs)
		{
			if (CreateDialogWidget != null)
			{
				CreateDialogWidget(this, eventArgs);
				return eventArgs.ResultDialogWidget;
			}
			else
				return null;
		}

		private void CloseTab(ITdiTab tab)
		{
			//Закрываем вкладку
			_tabs.Remove(tab);
			this.Remove((Widget)tab);
			(tab as Widget).Destroy();
			logger.Debug("Вкладка удалена");
		}
	}
}

