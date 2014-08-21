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
		private List<ITdiTab> _tabs;

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
			this.AppendPage((Widget)tab, box);
			this.ShowAll();
			this.ShowTabs = true;
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

			if(tab is ITdiDialog && (tab as ITdiDialog).HasChanges)
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
					if(!(tab as ITdiDialog).Save() )
					{
						logger.Warn("Вкладка не сохранена. Отмена закрытия...");
						return;
					}
				}
			}

			//Закрываем вкладку
			_tabs.Remove(tab);
			this.Remove((Widget)tab);
			(tab as Widget).Destroy();
		}


	}
}

