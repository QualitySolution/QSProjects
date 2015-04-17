using System;
using Gtk;
using System.ComponentModel;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DatePicker : Gtk.Bin
	{
		protected DateTime? date = null;
		public event EventHandler DateChanged;
		protected Dialog editDate;

		public DatePicker ()
		{
			this.Build ();
		}

		public string DateText {
			get {
				return entryDate.Text;
			}
		}

		public DateTime? DateOrNull {
			get {
				return date;
			}
			set {
				if (date == value)
					return;
				date = value;

				entryDate.Text = date.HasValue ? date.Value.ToShortDateString () : String.Empty;

				if(DateChanged != null)
					DateChanged(this, EventArgs.Empty);
			}
		}

		public DateTime Date {
			get {
				return date.GetValueOrDefault ();
			}
			set {
				if (value.Year == 1 && value.DayOfYear == 1)
					DateOrNull = null;
				else
				{
					DateOrNull = value;
				}
			}
		}

		public bool IsEmpty {
			get {
				return !date.HasValue;
			}

		}

		[DefaultValue (true)]
		public bool IsEditable{
			get { return entryDate.IsEditable;}
			set { entryDate.IsEditable = value;
				buttonEditDate.Sensitive = value;}
		}

		private bool _AutoSeparation = true;
		[System.ComponentModel.DefaultValue(true)]
		public bool AutoSeparation{
			get { return _AutoSeparation;}
			set { _AutoSeparation = value;}
		}

		protected void OnButtonEditDateClicked (object sender, EventArgs e)
		{
			Gtk.Window parentWin = (Gtk.Window) this.Toplevel;
			editDate = new Dialog("Укажите дату", parentWin, Gtk.DialogFlags.DestroyWithParent);
			editDate.Modal = true;
			editDate.AddButton ("Отмена", ResponseType.Cancel);
			editDate.AddButton ("Ok", ResponseType.Ok);
			Calendar SelectDate = new Calendar ();
			SelectDate.DisplayOptions = CalendarDisplayOptions.ShowHeading  | 
				CalendarDisplayOptions.ShowDayNames | 
					CalendarDisplayOptions.ShowWeekNumbers;
			SelectDate.DaySelectedDoubleClick += OnCalendarDaySelectedDoubleClick;
			SelectDate.Date = date ?? DateTime.Now.Date;
			
			editDate.VBox.Add(SelectDate);
			editDate.ShowAll();
			int response = editDate.Run ();
			if(response == (int)ResponseType.Ok)
			{
				DateOrNull = SelectDate.GetDate();
			}
			SelectDate.Destroy();
			editDate.Destroy ();
		}

		public void Clear()
		{
			DateOrNull = null;
		}

		protected void OnEntryDateFocusInEvent (object o, FocusInEventArgs args)
		{
			entryDate.SelectRegion(0,10);
		}

		protected void OnEntryDateFocusOutEvent (object o, FocusOutEventArgs args)
		{
			if(entryDate.Text == "")
			{
				DateOrNull = null;
				return;
			}

			DateTime outDate;
			if(DateTime.TryParse(((Entry)o).Text, out outDate))
			{
				DateOrNull = outDate;
			}
			else
			{
				entryDate.Text = date.Value.ToShortDateString();
			}
		}

		protected void OnEntryDateChanged (object sender, EventArgs e)
		{
			DateTime outDate;
			if(DateTime.TryParse(entryDate.Text, out outDate))
				entryDate.ModifyText(StateType.Normal);
			else
				entryDate.ModifyText(StateType.Normal, new Gdk.Color(255,0,0)); 
		}

		protected void OnCalendarDaySelectedDoubleClick (object sender, EventArgs e)
		{
			editDate.Respond(ResponseType.Ok);
		}

		protected void OnEntryDateTextInserted (object o, TextInsertedArgs args)
		{
			if(!_AutoSeparation)
				return;
			if(args.Length == 1 && 
			   (args.Position == 3 || args.Position == 6) && 
			   args.Text != System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator &&
			   args.Position == entryDate.Text.Length)
			{
				int Pos = args.Position - 1;
				entryDate.InsertText( System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, ref Pos);
				args.Position++;
			}
		}

		protected void OnEntryDateActivated(object sender, EventArgs e)
		{
			this.ChildFocus (DirectionType.TabForward);
		}
	}
}

