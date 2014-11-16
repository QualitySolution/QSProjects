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
			if (TabParent.CheckClosingSlaveTabs (this as ITdiTab))
				return;

			ITdiDialog dlg = sender as ITdiDialog;
			if(arg.AskSave && dlg.HasChanges)
			{
				string Message = "Объект изменён. Сохранить изменения перед закрытием?";
				MessageDialog md = new MessageDialog ( (Window)this.Toplevel, DialogFlags.Modal,
					MessageType.Question, 
					ButtonsType.YesNo,
					Message);
				md.AddButton ("Отмена", ResponseType.Cancel);
				int result = md.Run ();
				md.Destroy();
				if (result == (int)ResponseType.Cancel)
					return;
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

		public void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab, bool CanSlided = true)
		{
			if(masterTab == Journal || masterTab == ActiveDialog)
				TabParent.AddSlaveTab(this as ITdiTab, slaveTab);
			else
				TabParent.AddSlaveTab(masterTab, slaveTab);
		}

		public void AddTab(ITdiTab tab, ITdiTab afterTab, bool CanSlided = true)
		{
			if (CanSlided && afterTab == Journal && tab is ITdiDialog) {
				ActiveDialog = (ITdiDialog)tab;
				return;
			}

			if(afterTab == Journal || afterTab == ActiveDialog)
				TabParent.AddTab(tab, this as ITdiTab);
			else
				TabParent.AddTab(tab, afterTab);
		}

		public bool CheckClosingSlaveTabs(ITdiTab tab)
		{
			//FIXME Если появятся подчиненые вкладки у журналов, нужно переделать проверку, что бы при закрыти диалога не требовалось зарывать подчиненные вкладки журнала.
			if (tab == Journal || tab == ActiveDialog)
				return TabParent.CheckClosingSlaveTabs (this as ITdiTab);
			else
				return TabParent.CheckClosingSlaveTabs (tab);
		}

		public ITdiDialog OnCreateDialogWidget(TdiOpenObjDialogEventArgs eventArgs)
		{
			return TabParent.OnCreateDialogWidget(eventArgs);
		}
			
		public TdiBeforeCreateResultFlag BeforeCreateNewTab(object subject, ITdiTab masterTab, bool CanSlided = true)
		{

			TdiBeforeCreateResultFlag result = TabParent.BeforeCreateNewTab (subject, masterTab);
			if(CanSlided && ActiveDialog != null && result == TdiBeforeCreateResultFlag.Ok)
			{
				OnDialogClose(ActiveDialog, new TdiTabCloseEventArgs(true));
				if (ActiveDialog != null)
					result |= TdiBeforeCreateResultFlag.Canceled;
			}

			return result;
		}

		public TdiBeforeCreateResultFlag BeforeCreateNewTab(System.Type subjectType, ITdiTab masterTab, bool CanSlided = true)
		{
			if (subjectType == null) //Потому что при null, может вызваться эта функция.
				BeforeCreateNewTab ((object)null, masterTab, CanSlided);
			return TabParent.BeforeCreateNewTab (subjectType, masterTab);
		}
	}
}

