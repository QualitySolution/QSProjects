using System;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSOrmProject;
using QSTDI;
using System.Collections.Generic;
using QSContacts;

namespace QSContacts
{

	public partial class ContactDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private IContactOwn contactOwn;
		private Adaptor adaptor = new Adaptor();
		private Contact subject;

		OrmParentReference parentReference;
		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if(!(parentReference.ParentObject is IContactOwn))
					{
						throw new ArgumentException (String.Format("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IContactOwn)));
					}
					this.contactOwn = (IContactOwn)parentReference.ParentObject;
				}
			}
			get {
				return parentReference;
			}
		}

		public ContactDlg(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new Contact();
			contactOwn.Contacts.Add (subject);
			ConfigureDlg();
		}

		public ContactDlg(OrmParentReference parenReferance, Contact subject)
		{
			this.Build();
			ParentReference = parenReferance;
			this.subject = subject;
			TabName = subject.Surname + " " + subject.Name + " " + subject.Lastname;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entrySurname.IsEditable = entryName.IsEditable = entryLastname.IsEditable = entryPost.IsEditable = true;
			dataComment.Editable = true;
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			emailsView.Session = Session;
			if (subject.Emails == null)
				subject.Emails = new List<Email>();
			emailsView.Emails = subject.Emails;
			phonesView.Session = Session;
			if (subject.Phones == null)
				subject.Phones = new List<Phone>();
			phonesView.Phones = subject.Phones;
		}

		#region ITdiTab implementation
		public event EventHandler<QSTDI.TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<QSTDI.TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Новое контактное лицо";
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

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			logger.Info("Сохраняем контактное лицо...");
			phonesView.SaveChanges();
			emailsView.SaveChanges ();
			if(contactOwn != null)
				OrmMain.DelayedNotifyObjectUpdated (contactOwn, subject);
			return true;
		}

		public bool HasChanges {
			get {return Session.IsDirty();}
		}

		#endregion

		#region IOrmDialog implementation

		public NHibernate.ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set {
				session = value;
			}
		}

		public object Subject {
			get {return subject;}
			set {
				if (value is Contact)
					subject = value as Contact;
			}
		}
		#endregion

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab(false);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
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

