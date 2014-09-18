using System;
using NLog;
using System.Data.Bindings;
using NHibernate;
using QSTDI;
using QSOrmProject;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptorOrg = new Adaptor();
		private Account subject;
		private bool NewItem = false;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return NewItem || Session.IsDirty();}
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
		}

		public bool Save()
		{
			logger.Info("Сохраняем счет организации...");
			Session.SaveOrUpdate(subject);
			OrmMain.NotifyObjectUpdated(subject);
			logger.Info("Ok");
			return true;
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if (Save())
				OnCloseTab(false);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

	}
}

