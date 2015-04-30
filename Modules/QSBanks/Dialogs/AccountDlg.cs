using System;
using NLog;
using System.Data.Bindings;
using NHibernate;
using QSTDI;
using QSOrmProject;
using Gtk;
using QSValidation;
using NHibernate.Criterion;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountDlg : Bin, ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private IAccountOwner accountOwner;
		private Adaptor adaptorOrg = new Adaptor ();
		private Adaptor adaptorBank = new Adaptor ();

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public bool HasChanges { 
			get { return false; }
		}

		private string _tabName = "Новый счет";

		public string TabName {
			get { return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
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
					this.accountOwner = (IAccountOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
		}

		#region IOrmDialog implementation

		private ISession session;

		public NHibernate.ISession Session {
			get {
				if (session == null)
					Session = OrmMain.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		private Account subject;

		public object Subject {
			get { return subject; }
			set {
				if (value is Account) {
					subject = value as Account;
					if (subject.Inactive)
						labelInactive.Markup = "<span foreground=\"red\">Данный счет находится в более не существующем банке.</span>";
				}
			}
		}

		#endregion


		public AccountDlg (OrmParentReference parentReference)
		{
			this.Build ();
			ParentReference = parentReference;
			Subject = new Account ();
			accountOwner.Accounts.Add (subject);
			ConfigureDlg ();
		}

		public AccountDlg (OrmParentReference parentReference, Account sub)
		{
			this.Build ();
			ParentReference = parentReference;
			Subject = sub;
			TabName = subject.Name;
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			dataentryrefBank.SubjectType = typeof(Bank);
			adaptorOrg.Target = subject;
			datatableMain.DataSource = adaptorOrg;
			datatableBank.DataSource = adaptorBank;
			dataentryNumber.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryrefBank.ItemsCriteria = Session.CreateCriteria<Bank> ()
				.Add (Restrictions.Eq ("Deleted", false));
		}

		public bool Save ()
		{
			subject.Inactive = subject.InBank == null || subject.InBank.Deleted;
			var valid = new QSValidator<Account> (subject);
			if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем счет организации...");
			if (accountOwner != null)
				OrmMain.DelayedNotifyObjectUpdated (accountOwner, subject);
			logger.Info ("Ok");
			return true;
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		protected void OnCloseTab (bool askSave)
		{
			if (TabParent.CheckClosingSlaveTabs ((ITdiTab)this))
				return;

			if (Save () == false)
				return;

			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		public override void Destroy ()
		{
			base.Destroy ();
		}

		protected void OnDataentryrefBankChanged (object sender, EventArgs e)
		{
			adaptorBank.Target = subject.InBank;
			subject.Inactive = subject.InBank == null || subject.InBank.Deleted;				
			labelInactive.Markup = subject.Inactive ? "<span foreground=\"red\">Данный счет находится в более не существующем банке.</span>" : "";
			
		}
	}
}

