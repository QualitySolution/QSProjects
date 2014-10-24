using System;
using System.Data.Bindings;
using Gtk.DataBindings;
using NLog;
using NHibernate;
using System.Data.Bindings.Collections.Generic;
using System.Data.Bindings.Collections;
using System.Collections.Generic;
using Gtk;
using QSOrmProject;
using QSWidgetLib;

namespace QSPhones
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EmailsView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private GenericObservableList<Email> EmailsList;
		private Adaptor EmailTypesAdaptor = new Adaptor();

		public ISession Session
		{
			get {
				return session;
			}
			set {
				session = value;
				if (session != null) {
					var criteria = session.CreateCriteria<EmailType>();
					EmailTypesAdaptor.Target = new ObservableList (criteria.List ());
				}
			}
		}

		private IList<Email> emails;
		public IList<Email> Emails
		{
			get {
				return emails;
			}
			set {
				if (emails == value)
					return;
				if (EmailsList != null)
					CleanList();
				emails = value;
				buttonAdd.Sensitive = emails != null;
				if (value != null) {
					EmailsList = new GenericObservableList<Email> (emails);
					EmailsList.ElementAdded += OnEmailListElementAdded;
					EmailsList.ElementRemoved += OnEmailListElementRemoved;
					if (EmailsList.Count == 0)
						EmailsList.Add(new Email());
					else
					{
						foreach (Email email in EmailsList)
							AddEmailRow(email);
					}
				}
			}
		}

		void OnEmailListElementRemoved (object aList, int[] aIdx, object aObject)
		{
			Widget foundWidget = null;
			foreach(Widget wid in datatableEmails.AllChildren)
			{
				if(wid is IAdaptableContainer && (wid as IAdaptableContainer).Adaptor.Adaptor.FinalTarget == aObject)
				{
					foundWidget = wid;
					break;
				}
			}
			if(foundWidget == null)
			{
				logger.Warn("Не найден виджет ассоциированный с удаленным телефоном.");
				return;
			}

			Table.TableChild child = ((Table.TableChild)(this.datatableEmails [foundWidget]));
			RemoveRow(child.TopAttach);
		}

		void OnEmailListElementAdded (object aList, int[] aIdx)
		{
			foreach(int i in aIdx)
			{
				AddEmailRow(EmailsList[i]);
			}
		}

		uint RowNum;

		public EmailsView ()
		{
			this.Build ();
			datatableEmails.NRows = RowNum = 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			EmailsList.Add(new Email());
		}

		private void AddEmailRow(Email newEmail) 
		{
			datatableEmails.NRows = RowNum + 1;
			Adaptor rowAdaptor = new Adaptor(newEmail);

			DataComboBox emailDataCombo = new DataComboBox (rowAdaptor, "EmailType");
			emailDataCombo.WidthRequest = 100;
			emailDataCombo.ItemsDataSource = EmailTypesAdaptor;
			emailDataCombo.ColumnMappings = "{QSPhones.EmailType} Name[Имя]";
			datatableEmails.Attach (emailDataCombo, (uint)0, (uint)1, RowNum, RowNum + 1, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);

			DataValidatedEntry emailDataEntry = new DataValidatedEntry (rowAdaptor, "Address");
			emailDataEntry.ValidationMode = ValidationType.email;
			datatableEmails.Attach (emailDataEntry, (uint)1, (uint)2, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Button deleteButton = new Gtk.Button ();
			Gtk.Image image = new Gtk.Image ();
			image.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-delete", global::Gtk.IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatableEmails.Attach (deleteButton, (uint)2, (uint)3, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			datatableEmails.ShowAll ();

			RowNum++;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatableEmails [(Widget)sender]));
			Widget foundWidget = null;
			foreach(Widget wid in datatableEmails.AllChildren)
			{
				if(wid is IAdaptableContainer && delButtonInfo.TopAttach == (datatableEmails[wid] as Table.TableChild).TopAttach)
				{
					foundWidget = wid;
					break;
				}
			}
			if(foundWidget == null)
			{
				logger.Warn("Не найден виджет ассоциированный с удаленным телефоном.");
				return;
			}

			EmailsList.Remove((Email)(foundWidget as IAdaptableContainer).Adaptor.Adaptor.FinalTarget);
		}

		private void RemoveRow(uint Row)
		{
			foreach (Widget w in datatableEmails.Children)
				if (((Table.TableChild)(this.datatableEmails [w])).TopAttach == Row) {
					datatableEmails.Remove (w);
					w.Destroy ();
				}
			for (uint i = Row + 1; i < datatableEmails.NRows; i++)
				MoveRowUp (i);
			datatableEmails.NRows = --RowNum;
		}

		protected void MoveRowUp(uint Row)
		{
			foreach (Widget w in datatableEmails.Children)
				if (((Table.TableChild)(this.datatableEmails [w])).TopAttach == Row) {
					uint Left = ((Table.TableChild)(this.datatableEmails [w])).LeftAttach;
					uint Right = ((Table.TableChild)(this.datatableEmails [w])).RightAttach;
					datatableEmails.Remove (w);
					if (w.GetType() == typeof(DataComboBox))
						datatableEmails.Attach (w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
					else
						datatableEmails.Attach (w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
				}
		}

		private void CleanList()
		{
			while (EmailsList.Count > 0)
			{
				EmailsList.RemoveAt(0);
			}
		}

		public void SaveChanges()
		{
			foreach(Email email in EmailsList)
			{
				if(email.Address != "")
					Session.SaveOrUpdate(email);
			}
		}
	}
}

