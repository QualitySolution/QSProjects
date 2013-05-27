using System;
using Gtk;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DatePicker : Gtk.Bin
	{
		protected DateTime date;
		protected bool isEmpty;
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
		public DateTime Date {
			get {
				return date;
			}
			set {
				date = value;
				if(date.Year == 1 && date.DayOfYear == 1)
					Clear ();
				else
				{
					isEmpty = false;
					entryDate.Text = date.ToShortDateString();
					if(DateChanged != null)
						DateChanged(this, EventArgs.Empty);
				}
			}
		}

		public bool IsEmpty {
			get {
				return isEmpty;
			}

		}

		public bool IsEditable{
			get { return entryDate.IsEditable;}
			set { entryDate.IsEditable = value;
				buttonEditDate.Sensitive = value;}
		}

		public void Clear()
		{
			isEmpty = true;
			entryDate.Text = "";
			if(DateChanged != null)
				DateChanged(this, EventArgs.Empty);
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
			if(isEmpty)
				SelectDate.Date = DateTime.Now.Date;
			else
				SelectDate.Date = date;
			editDate.VBox.Add(SelectDate);
			editDate.ShowAll();
			int response = editDate.Run ();
			if(response == (int)ResponseType.Ok)
			{
				date = SelectDate.GetDate();
				isEmpty = false;
				entryDate.Text = date.ToShortDateString();
				if(DateChanged != null)
					DateChanged(this, e);
			}
			SelectDate.Destroy();
			editDate.Destroy ();
		}

		protected void OnEntryDateFocusInEvent (object o, FocusInEventArgs args)
		{
			entryDate.SelectRegion(1,10);
		}

		protected void OnEntryDateFocusOutEvent (object o, FocusOutEventArgs args)
		{
			DateTime outDate;
			if(entryDate.Text == "")
			{
				Clear();
				return;
			}
			if(DateTime.TryParse(((Entry)o).Text, out outDate))
			{
				entryDate.Text = outDate.ToShortDateString();
				date = outDate;
				isEmpty = false;
				if(DateChanged != null)
					DateChanged(this, args);
			}
			else
			{
				entryDate.Text = date.ToShortDateString();
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
	}
}

