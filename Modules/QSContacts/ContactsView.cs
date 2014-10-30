using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QSOrmProject;
using QSTDI;

namespace QSContacts
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ContactsView : Gtk.Bin
	{
		private IContact contact;
		private GenericObservableList<Contact> contactsList;
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

		public IContact Contact
		{
			get
			{
				return contact;
			}
			set
			{
				contact = value;
				if(Contact.Contacts == null)
					Contact.Contacts = new List<Contact>();
				contactsList = new GenericObservableList<Contact>(Contact.Contacts);
				datatreeviewContacts.ItemsDataSource = contactsList;
			}
		}

		public ContactsView()
		{
			this.Build();
			OrmMain.ClassMapingList.Find(m => m.ObjectClass == typeof(Contact)).ObjectUpdated += OnContactUpdated;
			datatreeviewContacts.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewContacts.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		void OnContactUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{

			if (Session == null)
				return;
			Session.Merge (e.Subject);
			//Session.Lock(e.Subject, LockMode.Read);
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			ContactDlg dlg = new ContactDlg(Session);
			contactsList.Add((Contact)dlg.Subject);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

		    ContactDlg dlg = new ContactDlg(Session, datatreeviewContacts.GetSelectedObjects()[0] as Contact);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnDatatreeviewAccountsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click();
		}
	}
}

