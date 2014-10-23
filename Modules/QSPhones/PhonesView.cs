using System;
using System.Text.RegularExpressions;
using Gtk;
using Gtk.DataBindings;
using NHibernate;
using System.Data.Bindings.Collections;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Data.Bindings;
using NLog;

namespace QSPhones
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PhonesView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private GenericObservableList<Phone> PhonesList;
		private Adaptor PhoneTypesAdaptor = new Adaptor();

		public ISession Session
		{
			get {
				return session;
			}
			set {
				session = value;
				if (session != null) {
					var criteria = session.CreateCriteria<PhoneType>();
					PhoneTypesAdaptor.Target = new ObservableList (criteria.List ());
				}
			}
		}

		private IList<Phone> phones;
		public IList<Phone> Phones
		{
			get {
				return phones;
			}
			set {
				if (phones == value)
					return;
				if (PhonesList != null)
					CleanList();
				phones = value;
				buttonAdd.Sensitive = phones != null;
				if (value != null) {
					PhonesList = new GenericObservableList<Phone> (phones);
					PhonesList.ElementAdded += OnPhoneListElementAdded;
					PhonesList.ElementRemoved += OnPhoneListElementRemoved;
					if (PhonesList.Count == 0)
						PhonesList.Add(new Phone());
					else
					{
						foreach (Phone phone in PhonesList)
							AddPhoneRow(phone);
					}
				}
			}
		}

		void OnPhoneListElementRemoved (object aList, int[] aIdx, object aObject)
		{
			Widget foundWidget = null;
			foreach(Widget wid in datatablePhones.AllChildren)
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

			Table.TableChild child = ((Table.TableChild)(this.datatablePhones [foundWidget]));
			RemoveRow(child.TopAttach);
		}

		void OnPhoneListElementAdded (object aList, int[] aIdx)
		{
			foreach(int i in aIdx)
			{
				AddPhoneRow(PhonesList[i]);
			}
		}

		uint RowNum;

		public PhonesView ()
		{
			this.Build ();
			datatablePhones.NRows = RowNum = 0;
		}

		protected void OnEntryPhoneTextInserted (object o, Gtk.TextInsertedArgs args)
		{
			FormatString (o);
			switch (args.Position) {
			case 1:
				args.Position += 1;
				break;
			case 5:
				args.Position += 2;
				break;
			case 10:
				args.Position += 3;
				break;
			case 15:
				args.Position += 3;
				break;
			}
		}

		protected void OnEntryPhoneTextDeleted (object o, Gtk.TextDeletedArgs args)
		{
			FormatString (o);
			DataEntry Entry = o as DataEntry;
			if (args.StartPos > Entry.Text.Length)
				Entry.Position = Entry.Text.Length;
			else
				Entry.Position = args.StartPos;
			if (args.StartPos == 16 && args.EndPos == 17) {			//Backspace
				Entry.Text = Entry.Text.Remove (13, 1);
				Entry.Position = 13;
			} else if (args.StartPos == 11 && args.EndPos == 12) {
				Entry.Text = Entry.Text.Remove (8, 1);
				Entry.Position = 8;
			} else if (args.StartPos == 5 && args.EndPos == 6) {
				Entry.Text = Entry.Text.Remove (3, 1);
				Entry.Position = 3;
			} else if (args.StartPos == 14 && args.EndPos == 15) { 	//Delete
				Entry.Text = Entry.Text.Remove (17, 1);
				Entry.Position = 17;
			} else if (args.StartPos == 9 && args.EndPos == 10) {
				Entry.Text = Entry.Text.Remove (12, 1);
				Entry.Position = 12;
			} else if (args.StartPos == 4 && args.EndPos == 5) {
				Entry.Text = Entry.Text.Remove (6, 1);
				Entry.Position = 6;
			}
		}

		private void FormatString(object o)
		{
			string Number = (o as DataEntry).Text;
			Number = Regex.Replace (Number, "[^0-9]", "");
			if (Number != String.Empty) {
				if (Number.Length > 0)
					Number = Number.Insert (0, "(");
				if (Number.Length > 4)
					Number = Number.Insert (4, ") ");
				if (Number.Length > 9)
					Number = Number.Insert (9, " - ");
				if (Number.Length > 14)
					Number = Number.Insert (14, " - ");
			}
			(o as DataEntry).Text = Number;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			PhonesList.Add(new Phone());
		}

		private void AddPhoneRow(Phone newPhone) 
		{
			datatablePhones.NRows = RowNum + 1;
			Adaptor rowAdaptor = new Adaptor(newPhone);

			DataComboBox phoneDataCombo = new DataComboBox (rowAdaptor, "NumberType");
			phoneDataCombo.WidthRequest = 100;
			phoneDataCombo.ItemsDataSource = PhoneTypesAdaptor;
			phoneDataCombo.ColumnMappings = "{QSPhones.PhoneType} Name[Имя]";
			datatablePhones.Attach (phoneDataCombo, (uint)0, (uint)1, RowNum, RowNum + 1, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label textPhoneLabel = new Gtk.Label ("+7");
			datatablePhones.Attach (textPhoneLabel, (uint)1, (uint)2, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataEntry phoneDataEntry = new DataEntry (rowAdaptor, "Number");
			phoneDataEntry.CanFocus = true;
			phoneDataEntry.IsEditable = true;
			phoneDataEntry.WidthChars = 19;
			phoneDataEntry.MaxLength = 19;
			phoneDataEntry.TextInserted += OnEntryPhoneTextInserted;
			phoneDataEntry.TextDeleted += OnEntryPhoneTextDeleted;
			datatablePhones.Attach (phoneDataEntry, (uint)2, (uint)3, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label textAdditionalLabel = new Gtk.Label ("доб.");
			datatablePhones.Attach (textAdditionalLabel, (uint)3, (uint)4, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataEntry additionalDataEntry = new DataEntry (rowAdaptor,"Additional");
			additionalDataEntry.WidthRequest = 50;
			datatablePhones.Attach (additionalDataEntry, (uint)4, (uint)5, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Button deleteButton = new Gtk.Button ();
			Gtk.Image image = new Gtk.Image ();
			image.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-delete", global::Gtk.IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatablePhones.Attach (deleteButton, (uint)5, (uint)6, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			datatablePhones.ShowAll ();

			RowNum++;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatablePhones [(Widget)sender]));
			Widget foundWidget = null;
			foreach(Widget wid in datatablePhones.AllChildren)
			{
				if(wid is IAdaptableContainer && delButtonInfo.TopAttach == (datatablePhones[wid] as Table.TableChild).TopAttach)
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

			PhonesList.Remove((Phone)(foundWidget as IAdaptableContainer).Adaptor.Adaptor.FinalTarget);
		}

		private void RemoveRow(uint Row)
		{
			foreach (Widget w in datatablePhones.Children)
				if (((Table.TableChild)(this.datatablePhones [w])).TopAttach == Row) {
					datatablePhones.Remove (w);
					w.Destroy ();
				}
			for (uint i = Row + 1; i < datatablePhones.NRows; i++)
				MoveRowUp (i);
			datatablePhones.NRows = --RowNum;
		}

		protected void MoveRowUp(uint Row)
		{
			foreach (Widget w in datatablePhones.Children)
				if (((Table.TableChild)(this.datatablePhones [w])).TopAttach == Row) {
					uint Left = ((Table.TableChild)(this.datatablePhones [w])).LeftAttach;
					uint Right = ((Table.TableChild)(this.datatablePhones [w])).RightAttach;
					datatablePhones.Remove (w);
					if (w.GetType() == typeof(DataComboBox))
						datatablePhones.Attach (w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
					else
						datatablePhones.Attach (w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
				}
		}

		private void CleanList()
		{
			while (PhonesList.Count > 0)
			{
				PhonesList.RemoveAt(0);
			}
		}

		public void SaveChanges()
		{
			foreach(Phone phone in PhonesList)
			{
				if(phone.Number != "")
					Session.SaveOrUpdate(phone);
			}
		}
	}
}

