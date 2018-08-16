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
		public event EventHandler DateChangedByUser;
		protected Dialog editDate;

		public DatePicker ()
		{
			this.Build ();
		}

		bool withTime;
		[Browsable(true)]
		[DefaultValue(false)]
		public bool WithTime
		{
			get
			{
				return withTime;
			}
			set
			{
				withTime = value;
				entryDate.MaxLength = entryDate.WidthChars = WithTime ? 16 : 10;
			}
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

				EntrySetDateTime(date);
				OnDateChanged ();
			}
		}

		protected virtual void OnDateChanged()
		{
			DateChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnDateChangedByUser()
		{
			DateChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		public DateTime Date {
			get {
				return date.GetValueOrDefault ();
			}
			set {
				if (value == default(DateTime))
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
			editDate = new Dialog(WithTime ? "Укажите дату и время" : "Укажите дату", 
				parentWin, Gtk.DialogFlags.DestroyWithParent);
			editDate.Modal = true;
			editDate.AddButton ("Отмена", ResponseType.Cancel);
			editDate.AddButton ("Ok", ResponseType.Ok);
			TimeEntry timeEntry = null;
			if(WithTime)
			{
				HBox timeBox = new HBox();
				Label timeLabel = new Label("Время:");
				timeLabel.Angle = 1;
				timeBox.Add(timeLabel);
				timeEntry = new TimeEntry();
				timeEntry.DateTime = date ?? DateTime.Now.Date;
				timeEntry.AutocompleteStep = 5;
				timeBox.Add(timeEntry);
				editDate.VBox.Add(timeBox);
			}
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
				DateOrNull = WithTime ? SelectDate.GetDate() + timeEntry.Time : SelectDate.GetDate();
				OnDateChangedByUser();
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
				OnDateChangedByUser();
				return;
			}

			DateTime outDate;
			if(DateTime.TryParse(entryDate.Text, out outDate))
			{
				DateOrNull = outDate;
				OnDateChangedByUser();
			}
			else
			{
				EntrySetDateTime(DateOrNull);
			}
		}

		void EntrySetDateTime(DateTime? date)
		{
			if(date.HasValue)
				entryDate.Text = WithTime ?  date.Value.ToString("g") : date.Value.ToShortDateString();
			else
				entryDate.Text = String.Empty;
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

		public void ModifyBase(StateType state, Gdk.Color color){
			entryDate.ModifyBase(state, color);
		}
	}
}

