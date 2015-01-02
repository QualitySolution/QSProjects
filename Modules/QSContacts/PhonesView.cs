using System;
using System.Collections.Generic;
using System.Data.Bindings;
using System.Data.Bindings.Collections;
using System.Data.Bindings.Collections.Generic;
using System.Text.RegularExpressions;
using Gtk;
using Gtk.DataBindings;
using NHibernate;
using NLog;
using QSOrmProject;
using QSWidgetLib;
using QSSupportLib;

namespace QSContacts
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
			phoneDataCombo.ColumnMappings = "{QSContacts.PhoneType} Name[Имя]";
			datatablePhones.Attach (phoneDataCombo, (uint)0, (uint)1, RowNum, RowNum + 1, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label textPhoneLabel = new Gtk.Label ("+7");
			datatablePhones.Attach (textPhoneLabel, (uint)1, (uint)2, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataValidatedEntry phoneDataEntry = new DataValidatedEntry (rowAdaptor, "Number");
			phoneDataEntry.ValidationMode = ValidationType.phone;
			if (MainSupport.BaseParameters.All.ContainsKey ("default_city_code") && newPhone.DigitsNumber == String.Empty)
				phoneDataEntry.SetDefaultCityCode (MainSupport.BaseParameters.All["default_city_code"]);
			phoneDataEntry.WidthChars = 19;
			datatablePhones.Attach (phoneDataEntry, (uint)2, (uint)3, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label textAdditionalLabel = new Gtk.Label ("доб.");
			datatablePhones.Attach (textAdditionalLabel, (uint)3, (uint)4, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataEntry additionalDataEntry = new DataEntry (rowAdaptor,"Additional");
			additionalDataEntry.WidthRequest = 50;
			additionalDataEntry.MaxLength = 10;
			datatablePhones.Attach (additionalDataEntry, (uint)4, (uint)5, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

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

