using System;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSTDI;
using QSOrmProject;
using QSValidation;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BankDlg : Gtk.Bin, QSTDI.ITdiDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptorOrg = new Adaptor();
		private Bank subject;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return Session.IsDirty();}
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
			subject = new Bank();
			Session.Persist (subject);
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
			var valid = new QSValidator<Bank> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем банк...");
			try
			{
				Session.Flush();
			}
			catch( Exception ex)
			{
				logger.ErrorException("Не удалось записать банк.", ex);
				return false;
			}
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

