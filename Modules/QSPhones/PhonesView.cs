using System;
using System.Text.RegularExpressions;
using Gtk;
using Gtk.DataBindings;
using NHibernate;
using System.Data.Bindings.Collections;

namespace QSPhones
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PhonesView : Gtk.Bin
	{
		private ISession session;
		private ObservableList list;

		public ISession Session
		{
			get {
				return session;
			}
			set {
				session = value;
				if (session != null) {
					var criteria = session.CreateCriteria<PhoneType>();
					list = new ObservableList (criteria.List ());
					comboType.ItemsDataSource = list;
				}
			}
		}


		uint RowNum;

		public PhonesView ()
		{
			this.Build ();
			entryPhone.IsEditable = true;
			entryAdditional.IsEditable = true;
			entryPhone.MaxLength = 19;
			comboType.Sensitive = true;
			RowNum = datatable1.NRows;
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
			datatable1.NRows = RowNum + 1;

			DataComboBox phoneDataCombo = new DataComboBox ();
			phoneDataCombo.InheritedDataSource = false;
			phoneDataCombo.InheritedBoundaryDataSource = false;
			phoneDataCombo.CursorPointsEveryType = false;
			phoneDataCombo.InheritedDataSource = false;
			phoneDataCombo.InheritedBoundaryDataSource = false;
			phoneDataCombo.WidthRequest = 100;
			phoneDataCombo.ItemsDataSource = list;
			phoneDataCombo.Mappings = "NumberType";
			phoneDataCombo.ColumnMappings = "{QSPhones.PhoneType} Name[Имя]";
			datatable1.Attach (phoneDataCombo, (uint)0, (uint)1, RowNum, RowNum + 1, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);


			Gtk.Label textPhoneLabel = new Gtk.Label ("+7");
			datatable1.Attach (textPhoneLabel, (uint)1, (uint)2, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataEntry phoneDataEntry = new DataEntry ();
			phoneDataEntry.CanFocus = true;
			phoneDataEntry.IsEditable = true;
			phoneDataEntry.WidthChars = 19;
			phoneDataEntry.MaxLength = 19;
			phoneDataEntry.InheritedDataSource = false;
			phoneDataEntry.InheritedBoundaryDataSource = false;
			phoneDataEntry.InheritedDataSource = false;
			phoneDataEntry.InheritedBoundaryDataSource = false;
			phoneDataEntry.TextInserted += OnEntryPhoneTextInserted;
			phoneDataEntry.TextDeleted += OnEntryPhoneTextDeleted;
			datatable1.Attach (phoneDataEntry, (uint)2, (uint)3, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label textAdditionalLabel = new Gtk.Label ("доб.");
			datatable1.Attach (textAdditionalLabel, (uint)3, (uint)4, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataEntry additionalDataEntry = new DataEntry ();
			additionalDataEntry.WidthRequest = 50;
			additionalDataEntry.CanFocus = true;
			additionalDataEntry.IsEditable = true;
			additionalDataEntry.InheritedDataSource = false;
			additionalDataEntry.InheritedBoundaryDataSource = false;
			additionalDataEntry.InheritedDataSource = false;
			additionalDataEntry.InheritedBoundaryDataSource = false;
			datatable1.Attach (additionalDataEntry, (uint)4, (uint)5, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Button deleteButton = new Gtk.Button ();
			deleteButton.CanFocus = true;
			deleteButton.UseUnderline = true;
			Gtk.Image image = new Gtk.Image ();
			image.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-delete", global::Gtk.IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatable1.Attach (deleteButton, (uint)5, (uint)6, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			datatable1.ShowAll ();

			RowNum++;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			uint Row;
			Table.TableChild child = ((Table.TableChild)(this.datatable1 [(Widget)sender]));
			Row = child.TopAttach;
			foreach (Widget w in datatable1.Children)
				if (((Table.TableChild)(this.datatable1 [w])).TopAttach == Row) {
					datatable1.Remove (w);
					w.Destroy ();
				}
			for (uint i = Row + 1; i < datatable1.NRows; i++)
				MoveRowUp (i);
			datatable1.NRows = --RowNum;
		}

		protected void MoveRowUp(uint Row)
		{
			foreach (Widget w in datatable1.Children)
				if (((Table.TableChild)(this.datatable1 [w])).TopAttach == Row) {
					uint Left = ((Table.TableChild)(this.datatable1 [w])).LeftAttach;
					uint Right = ((Table.TableChild)(this.datatable1 [w])).RightAttach;
					datatable1.Remove (w);
					if (w.GetType() == typeof(DataComboBox))
						datatable1.Attach (w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
					else
						datatable1.Attach (w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
				}
		}
	}
}

