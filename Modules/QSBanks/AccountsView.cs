using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QSOrmProject;
using QSTDI;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountsView : Gtk.Bin
	{
		private IAccountOwner accountOwner;
		private GenericObservableList<Account> accountsList;
		private ISession session;

		public ISession Session
		{
			get
			{
				return session;
			}
			set
			{
				session = value;
			}
		}

		public IAccountOwner AccountOwner
		{
			get
			{
				return accountOwner;
			}
			set
			{
				accountOwner = value;
				if(AccountOwner.Accounts == null)
					AccountOwner.Accounts = new List<Account>();
				accountsList = new GenericObservableList<Account>(AccountOwner.Accounts);
				datatreeviewAccounts.ItemsDataSource = accountsList;
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

		public AccountsView()
		{
			this.Build();
			OrmMain.ClassMapingList.Find(m => m.ObjectClass == typeof(Account)).ObjectUpdated += OnAccountUpdated;
			datatreeviewAccounts.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewAccounts.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
			buttonDefault.Sensitive = selected && !(datatreeviewAccounts.GetSelectedObjects()[0] as Account).IsDefault;
		}

		void OnAccountUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			if (Session == null)
				return;
			Session.Lock(e.Subject, LockMode.Read);
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;
				
			AccountDlg dlg = new AccountDlg(ParentReference);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			AccountDlg dlg = new AccountDlg(ParentReference, datatreeviewAccounts.GetSelectedObjects()[0] as Account);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnDatatreeviewAccountsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		protected void OnButtonDefaultClicked(object sender, EventArgs e)
		{
			AccountOwner.DefaultAccount = (datatreeviewAccounts.GetSelectedObjects()[0] as Account);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

