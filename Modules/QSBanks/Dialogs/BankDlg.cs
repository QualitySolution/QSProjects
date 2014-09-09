using System;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSTDI;
using QSOrmProject;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BankDlg : Gtk.Bin, QSTDI.ITdiDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptorOrg = new Adaptor();
		private Bank subject;
		private bool NewItem = false;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return NewItem || Session.IsDirty();}
		}

		private string _tabName = "Новый банк";
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
				if (value is Bank)
					subject = value as Bank;
			}
		}

		public BankDlg()
		{
			this.Build();
			NewItem = true;
			subject = new Bank();
			ConfigureDlg();
		}

		public BankDlg(int id)
		{
			this.Build();
			subject = Session.Load<Bank>(id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public BankDlg(Bank sub)
		{
			this.Build();
			subject = Session.Load<Bank>(sub.Id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			adaptorOrg.Target = subject;
			datatableInfo.DataSource = adaptorOrg;
		}

		public bool Save()
		{
			logger.Info("Сохраняем банк...");
			Session.SaveOrUpdate(subject);
			Session.Flush();
			OrmMain.NotifyObjectUpdated(subject);
			logger.Info("Ok");
			return true;
		}

		public override void Destroy()
		{
			Session.Close();
			//adaptorOrg.Disconnect();
			base.Destroy();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
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

