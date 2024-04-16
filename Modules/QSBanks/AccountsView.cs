using System;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Tdi;
using QS.Dialog.Gtk;
using QS.Banks.Domain;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountsView : Gtk.Bin
	{
		private IAccountOwner accountOwner;

		public IUnitOfWork UoW { get; set; }

		private bool canEdit;
		public bool CanEdit {
			get => canEdit;
			set {
				canEdit = value;
				UpdateSensitivity();
			}
		}

		public void SetAccountOwner(IUnitOfWork uow, IAccountOwner accountOwner)
        {
			UoW = uow;
			this.accountOwner = accountOwner;
			UpdateAccounts();
		}

		private void UpdateAccounts()
        {
			datatreeviewAccounts.ColumnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<Account>.Create()
				.AddColumn("Основной").AddToggleRenderer(node => node.IsDefault).Editing().Radio()
				.AddColumn("Псевдоним").AddTextRenderer(node => node.Name)
				.AddColumn("В банке").AddTextRenderer(a => a.InBank != null ? a.InBank.Name : "нет")
				.AddColumn("Номер").AddTextRenderer(a => a.Number)
				.RowCells().AddSetter<CellRendererText>((c, a) => c.ForegroundGdk = a.Inactive
					? Rc.GetStyle(datatreeviewAccounts).Text(StateType.Insensitive)
					: Rc.GetStyle(datatreeviewAccounts).Text(StateType.Normal))
				.Finish();
			datatreeviewAccounts.ItemsDataSource = accountOwner.Accounts;
		}

		public AccountsView ()
		{
			this.Build ();
			CanEdit = true;
			datatreeviewAccounts.Selection.Changed += OnSelectionChanged;
		}

		private void UpdateSensitivity()
		{
			buttonAdd.Sensitive = CanEdit;
			buttonEdit.Sensitive = CanEdit;
			buttonDelete.Sensitive = CanEdit;
		}

		/// <summary>
		/// Установка заголовка окна, если не подходит значение по-умолчанию.
		/// </summary>
		/// <param name="title">Новый заголовок.</param>
		public void SetTitle (string title)
		{
			labelTitle.Text = title;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewAccounts.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected && CanEdit;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = DialogHelper.FindParentTab (this);
			if (mytab == null)
				return;
			Account newAccount = new Account();
			AccountDlg dlg = new AccountDlg (UoW, newAccount);
			dlg.AccountSaved += (s, savedAccount) =>
			{
				accountOwner.Accounts.Add(savedAccount);
			};
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = DialogHelper.FindParentTab (this);
			if (mytab == null)
				return;
			Account selectedAccount = datatreeviewAccounts.GetSelectedObjects()[0] as Account;
            if(selectedAccount == null)
            {
				return;
            }
			AccountDlg dlg = new AccountDlg(UoW, selectedAccount);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnDatatreeviewAccountsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			if(!CanEdit) {
				return;
			}
			buttonEdit.Click();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = DialogHelper.FindParentTab (this);
			if (mytab == null)
				return;

			if (OrmMain.DeleteObject (typeof(Account), (datatreeviewAccounts.GetSelectedObjects () [0] as Account).Id))
				accountOwner.Accounts.Remove (datatreeviewAccounts.GetSelectedObjects () [0] as Account);
		}
	}
}

