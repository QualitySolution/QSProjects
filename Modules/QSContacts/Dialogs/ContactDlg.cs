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
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ContactDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptor = new Adaptor();
		private Contact subject;
		private bool NewItem = false;

		public ContactDlg()
		{
			this.Build();
			NewItem = true;
			subject = new Contact();
			ConfigureDlg();
		}

		public ContactDlg(int id)
		{
			this.Build();
			subject = Session.Load<Contact>(id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public ContactDlg(Contact sub)
		{
			this.Build();
			subject = Session.Load<Contact> (sub.Id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public ContactDlg(ISession parentSession)
		{
			this.Build();
			Session = parentSession;
			NewItem = true;
			subject = new Contact();
			ConfigureDlg();
		}

		public ContactDlg(ISession parentSession, Contact sub)
		{
			this.Build();
			Session = parentSession;
			subject = sub;
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entryName.IsEditable = true;
			entryComment.IsEditable = true;
			entryPost.IsEditable = true;
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			enumFired.DataSource = adaptor;
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
			Session.SaveOrUpdate(subject);
			phonesView.SaveChanges();
			emailsView.SaveChanges ();
			return true;
		}

		public bool HasChanges {
			get {return NewItem || Session.IsDirty();}
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

