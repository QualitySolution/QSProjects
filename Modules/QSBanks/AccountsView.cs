using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QSOrmProject;
using QSTDI;
using Gtk;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountsView : Gtk.Bin
	{
		private IAccountOwner accountOwner;
		private GenericObservableList<Account> accountsList;
		private ISession session;

		public ISession Session {
			get { return session; }
			set { session = value; }
		}

		public IAccountOwner AccountOwner {
			get { return accountOwner; }
			set {
				accountOwner = value;
				if (AccountOwner.Accounts == null)
					AccountOwner.Accounts = new List<Account> ();
				accountsList = new GenericObservableList<Account> (AccountOwner.Accounts);
				datatreeviewAccounts.ItemsDataSource = accountsList;
				datatreeviewAccounts.Columns [1].SetCellDataFunc (datatreeviewAccounts.Columns [1].Cells [0], new Gtk.TreeCellDataFunc (RenderAccountName));
				datatreeviewAccounts.Columns [2].SetCellDataFunc (datatreeviewAccounts.Columns [2].Cells [0], new Gtk.TreeCellDataFunc (RenderAccountName));
				datatreeviewAccounts.Columns [3].SetCellDataFunc (datatreeviewAccounts.Columns [3].Cells [0], new Gtk.TreeCellDataFunc (RenderAccountName));
				datatreeviewAccounts.Columns [4].Visible = false;
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
			get { return parentReference; }
		}

		public AccountsView ()
		{
			this.Build ();
			datatreeviewAccounts.Selection.Changed += OnSelectionChanged;
		}

		private void RenderAccountName (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			if ((bool)model.GetValue (iter, 4) == true)
				(cell as CellRendererText).Foreground = "grey";
			else
				(cell as CellRendererText).Foreground = "black";
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewAccounts.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
			buttonDefault.Sensitive = selected && !(datatreeviewAccounts.GetSelectedObjects () [0] as Account).IsDefault;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;
				
			Account subject = new Account ();
			accountsList.Add (subject);

			AccountDlg dlg = new AccountDlg (ParentReference, subject);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			AccountDlg dlg = new AccountDlg (ParentReference, datatreeviewAccounts.GetSelectedObjects () [0] as Account);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnDatatreeviewAccountsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDefaultClicked (object sender, EventArgs e)
		{
			AccountOwner.DefaultAccount = (datatreeviewAccounts.GetSelectedObjects () [0] as Account);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			accountsList.Remove (datatreeviewAccounts.GetSelectedObjects () [0] as Account);
		}
	}
}

