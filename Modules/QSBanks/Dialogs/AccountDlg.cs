using System;
using NLog;
using System.Data.Bindings;
using NHibernate;
using QSTDI;
using QSOrmProject;
using Gtk;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptorOrg = new Adaptor();
		private Adaptor adaptorBank = new Adaptor();
		private Account subject;
		private bool NewItem = false;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return false;}
		}

		private string _tabName = "Новый счет";
		public string TabName
		{
			get{return _tabName;}
			set{
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}

		}

		public ISession Session
		{
			get
			{
				if (session == null)
					session = OrmMain.Sessions.OpenSession();
				return session;
			}
			set
			{
				session = value;
			}
		}

		public object Subject
		{
			get {return subject;}
			set {
				if (value is Account)
					subject = value as Account;
			}
		}


		public AccountDlg(ISession parentSession)
		{
			this.Build();
			Session = parentSession;
			NewItem = true;
			subject = new Account();
			ConfigureDlg();
		}

		public AccountDlg(int id)
		{
			this.Build();
			subject = Session.Load<Account>(id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public AccountDlg(ISession parentSession, Account sub)
		{
			this.Build();
			Session = parentSession;
			subject = sub;
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dataentryrefBank.SubjectType = typeof(Bank);
			adaptorOrg.Target = subject;
			datatableMain.DataSource = adaptorOrg;
			datatableBank.DataSource = adaptorBank;
			dataentryNumber.MaxLength = 25;
		}

		public bool Save()
		{
			if(dataentryrefBank.Subject == null)
			{
				logger.Warn("В счете незаполнен банк, счет не сохраняем.");
				return false;
			}

			logger.Info("Сохраняем счет организации...");
			Session.SaveOrUpdate(subject);
			OrmMain.NotifyObjectUpdated(subject);
			logger.Info("Ok");
			return true;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if(dataentryrefBank.Subject == null)
			{
				string Message = "В счете незаполнен банк, счет не будет сохранен. Всеравно закрыть вкладку?";
				MessageDialog md = new MessageDialog ((Window)this.Toplevel, DialogFlags.Modal,
					MessageType.Question, 
					ButtonsType.YesNo,
					Message);
				int result = md.Run ();
				md.Destroy ();
				if (result == (int)ResponseType.No)
					return;
			}

			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

		public override void Destroy()
		{
			Save();
			base.Destroy();
		}

		protected void OnDataentryrefBankChanged(object sender, EventArgs e)
		{
			adaptorBank.Target = subject.InBank;
		}
	}
}

