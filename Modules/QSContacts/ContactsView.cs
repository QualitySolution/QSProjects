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
		private IContactOwn contactOwn;
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

		public IContactOwn ContactOwn
		{
			get
			{
				return contactOwn;
			}
			set
			{
				contactOwn = value;
				if(ContactOwn.Contacts == null)
					ContactOwn.Contacts = new List<Contact>();
				contactsList = new GenericObservableList<Contact>(ContactOwn.Contacts);
				datatreeviewContacts.ItemsDataSource = contactsList;
			}
		}

		OrmParentReference parentReference;
		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IContactOwn)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IContactOwn)));
					}
					ContactOwn = (IContactOwn)parentReference.ParentObject;
				}
			}
			get {
				return parentReference;
			}
		}

		public ContactsView()
		{
			this.Build();
			datatreeviewContacts.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewContacts.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			var newContact = new Contact ();
			contactsList.Add(newContact);
			ContactDlg dlg = new ContactDlg(ParentReference, newContact);

			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			ContactDlg dlg = new ContactDlg(ParentReference, datatreeviewContacts.GetSelectedObjects()[0] as Contact);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnDatatreeviewAccountsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			contactsList.Remove (datatreeviewContacts.GetSelectedObjects () [0] as Contact);
		}
	}
}

