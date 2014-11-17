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
	public partial class AccountDlg : Gtk.Bin, QSTDI.ITdiDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession Session;
		private IAccountOwner AccountOwner;
		private Adaptor adaptorOrg = new Adaptor();
		private Adaptor adaptorBank = new Adaptor();
		private Account subject;

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

		OrmParentReference parentReference;
		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IAccountOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IAccountOwner)));
					}
					AccountOwner = (IAccountOwner)parentReference.ParentObject;
				}
			}
			get {
				return parentReference;
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


		public AccountDlg(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new Account();
			AccountOwner.Accounts.Add (subject);
			ConfigureDlg();
		}

		public AccountDlg(OrmParentReference parentReference, Account sub)
		{
			this.Build();
			ParentReference = parentReference;
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
			dataentryNumber.ValidationMode = QSWidgetLib.ValidationType.numeric;
		}

		public bool Save()
		{
			if(dataentryrefBank.Subject == null)
			{
				logger.Warn("В счете незаполнен банк, счет не сохраняем.");
				return false;
			}

			logger.Info("Сохраняем счет организации...");
			OrmMain.DelayedNotifyObjectUpdated(ParentReference.ParentObject, subject);
			logger.Info("Ok");
			return true;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (TabParent.CheckClosingSlaveTabs ((ITdiTab)this))
				return;

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

			Save ();

			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		protected void OnDataentryrefBankChanged(object sender, EventArgs e)
		{
			adaptorBank.Target = subject.InBank;
		}
	}
}

