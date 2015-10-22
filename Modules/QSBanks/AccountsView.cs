using System;
using System.Data.Bindings.Collections.Generic;
using Gtk;
using Gtk.DataBindings;
using QSOrmProject;
using QSTDI;

namespace QSBanks
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountsView : Gtk.Bin
	{
		private IAccountOwner accountOwner;

		public IUnitOfWork UoW { get; set; }

		IParentReference<Account> parentReference;

		public IParentReference<Account> ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					UoW = parentReference.ParentUoW;
					datatreeviewAccounts.ColumnMappingConfig = FluentMappingConfig<Account>.Create ()
						.AddColumn ("Осн.").SetDataProperty (node => node.IsDefault)
						.AddColumn ("Псевдоним").SetDataProperty (node => node.Name)
						.AddColumn ("В банке").AddTextRenderer (a => a.InBank != null ? a.InBank.Name : "нет")
						.AddColumn ("Номер").AddTextRenderer (a => a.Number)
						.RowCells ().AddSetter<CellRendererText> ((c, a) => c.Foreground = a.Inactive ? "grey" : "black")
						.Finish ();
					if (!(parentReference.ParentObject is IAccountOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IAccountOwner)));
					}
					accountOwner = (IAccountOwner)parentReference.ParentObject;
					datatreeviewAccounts.ItemsDataSource = accountOwner.ObservableAccounts;
				}
			}
			get { return parentReference; }
		}

		public AccountsView ()
		{
			this.Build ();
			datatreeviewAccounts.Selection.Changed += OnSelectionChanged;
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
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
			buttonDefault.Sensitive = selected && !(datatreeviewAccounts.GetSelectedObjects () [0] as Account).IsDefault;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;
				
			AccountDlg dlg = new AccountDlg (ParentReference);
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
			accountOwner.DefaultAccount = (datatreeviewAccounts.GetSelectedObjects () [0] as Account);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;
			
			//FIXME добавить проверку на корретное удаление зависимостей.
			accountOwner.ObservableAccounts.Remove (datatreeviewAccounts.GetSelectedObjects () [0] as Account);
		}
	}
}

