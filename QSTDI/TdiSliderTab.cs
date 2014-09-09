using System;
using Gtk;
using NLog;

namespace QSTDI
{
	public class TdiSliderTab : Gtk.HBox, ITdiTab, ITdiTabParent
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ITdiJournal journal;
		private ITdiDialog activeDialog;
		private VSeparator separator;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public string TabName
		{
			get{
				if (journal != null)
					return journal.TabName;
				else
					return "Пустой слайдер";
			}
			set{
				throw new NotSupportedException("Нельзя напрямую задавать имя вкладки.");
			}

		}

		public ITdiJournal Journal
		{
			get
			{
				return journal;
			}
			set
			{
				if (journal != value)
				{
					if(journal != null)
					{
						journal.TabNameChanged -= OnJournalTabNameChanged;
						journal.OpenObjDialog -= OnJournalOpenObjDialog;
						journal.CloseTab -= OnJournalClose;
						this.Remove((Widget)journal);
					}
					journal = value;
					journal.TabNameChanged += OnJournalTabNameChanged;
					journal.OpenObjDialog += OnJournalOpenObjDialog;
					journal.CloseTab += OnJournalClose;
					this.PackStart((Widget)Journal);
					(Journal as Widget).Show();
					journal.TabParent = this;
				}
			}
		}

		void OnJournalOpenObjDialog (object sender, TdiOpenObjDialogEventArgs e)
		{
			if(ActiveDialog != null)
			{
				OnDialogClose(ActiveDialog, new TdiTabCloseEventArgs(true));
				if (ActiveDialog != null)
					return;
			}
			ActiveDialog = (this.Parent as TdiNotebook).OnCreateDialogWidget(e);
		}

		public ITdiDialog ActiveDialog
		{
			get
			{
				return activeDialog;
			}
			set
			{
				if (activeDialog != value)
				{
					if(activeDialog != null)
					{
						activeDialog.CloseTab -= OnDialogClose;
						this.Remove((Widget)activeDialog);
						this.Remove(separator);
					}
					activeDialog = value;
					if (value != null)
					{
						activeDialog.CloseTab += OnDialogClose;
						separator = new VSeparator();
						separator.Show();
						this.PackEnd((Widget)activeDialog);
						this.PackEnd(separator, false, true, 6);
						(ActiveDialog as Widget).Show();
						activeDialog.TabParent = this;
					}
				}

			}
		}

		void OnJournalTabNameChanged(object sender, TdiTabNameChangedEventArgs arg)
		{
			if (TabNameChanged != null)
				TabNameChanged(this, arg);
		}

		protected void OnJournalClose(object sender, TdiTabCloseEventArgs arg)
		{
			if (CloseTab != null)
				CloseTab(this, arg);
		}

		protected void OnDialogClose(object sender, TdiTabCloseEventArgs arg)
		{
			ITdiDialog dlg = sender as ITdiDialog;
			if(arg.AskSave && dlg.HasChanges)
			{
				string Message = "Объект изменён. Сохранить изменения перед закрытием?";
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
						logger.Warn("Объект не сохранён. Отмена закрытия...");
						return;
					}
				}
			}
			ActiveDialog = null;
			(dlg as Widget).Destroy();
		}

		public TdiSliderTab(ITdiJournal jour)
		{
			Journal = jour;
		}

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab)
		{
			if(masterTab == Journal || masterTab == ActiveDialog)
				TabParent.AddSlaveTab(this as ITdiTab, slaveTab);
			else
				TabParent.AddSlaveTab(masterTab, slaveTab);
		}
	}
}

