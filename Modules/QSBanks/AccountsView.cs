using System;
using QSOrmProject;
using NHibernate;
using QSTDI;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountsView : Gtk.Bin
	{
		private IAccountOwner accountOwner;
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
				datatreeviewAccounts.DataSource = AccountOwner.Accounts;
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
		}

		void OnAccountUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			Session.Refresh(e.Subject);
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			AccountDlg dlg = new AccountDlg();
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			AccountDlg dlg = new AccountDlg(datatreeviewAccounts.GetSelectedObjects()[0] as Account);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}
	}
}

