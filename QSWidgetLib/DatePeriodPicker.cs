using System;
using Gtk;
using System.Text.RegularExpressions;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DatePeriodPicker : Bin
	{
		#region Fields

		protected DateTime? startDate = null;

		public DateTime StartDate { 
			get { return !startDate.HasValue ? DateTime.Now : startDate.Value.Date; }
			set { 
				if (value != default(DateTime)) {
					startDate = value;
					OnDateChanged ();
				}
			}
		}

		private DateTime? endDate = null;

		public DateTime EndDate { 
			get { return !endDate.HasValue ? DateTime.Now : endDate.Value.Date; }
			set { 
				if (value != default(DateTime)) {
					endDate = value; 
					OnDateChanged ();
				}
			}
		}

		CalendarDisplayOptions DisplayOptions = CalendarDisplayOptions.ShowHeading | CalendarDisplayOptions.ShowDayNames;

		#endregion

		#region Events

		public event EventHandler DateChanged;

		private void OnDateChanged ()
		{
			if (DateChanged != null)
				DateChanged (this, EventArgs.Empty);
		}

		#endregion

		public DatePeriodPicker ()
		{
			this.Build ();
			UpdateEntryText ();
		}

		private void  UpdateEntryText ()
		{
			entryDate.Text = String.Format ("{0} - {1}", StartDate.ToShortDateString (), EndDate.ToShortDateString ());
		}

		#region Event handlers

		protected void OnButtonPickDatePeriodClicked (object sender, EventArgs e)
		{
			#region Widget creation
			Window parentWin = (Window)Toplevel;
			var selectDate = new Dialog ("Укажите дату", parentWin, DialogFlags.DestroyWithParent);
			selectDate.Modal = true;
			selectDate.AddButton ("Отмена", ResponseType.Cancel);
			selectDate.AddButton ("Ok", ResponseType.Ok);

			HBox hbox = new HBox (true, 6);

			Calendar StartDateCalendar = new Calendar ();
			StartDateCalendar.DisplayOptions = DisplayOptions;
			StartDateCalendar.Date = StartDate;

			Calendar EndDateCalendar = new Calendar ();
			EndDateCalendar.DisplayOptions = DisplayOptions;
			EndDateCalendar.Date = EndDate;

			hbox.Add (StartDateCalendar);
			hbox.Add (EndDateCalendar);

			selectDate.VBox.Add (hbox);
			selectDate.ShowAll ();
			#endregion

			int response = selectDate.Run ();
			if (response == (int)ResponseType.Ok) {
				StartDate = StartDateCalendar.GetDate ();
				EndDate = EndDateCalendar.GetDate ();
				UpdateEntryText ();
			}

			#region Destroy
			EndDateCalendar.Destroy ();
			StartDateCalendar.Destroy ();
			hbox.Destroy ();
			selectDate.Destroy ();
			#endregion
		}

		protected void OnEntryDateActivated (object sender, EventArgs e)
		{
			//throw new NotImplementedException ();
		}

		protected void OnEntryDateChanged (object sender, EventArgs e)
		{
			//throw new NotImplementedException ();
		}

		protected void OnEntryDateTextInserted (object o, TextInsertedArgs args)
		{
			//throw new NotImplementedException ();
		}

		protected void OnEntryDateFocusOutEvent (object o, FocusOutEventArgs args)
		{
			if (entryDate.Text == "") {
				startDate = null;
				endDate = null;
				return;
			}

			DateTime start, end;

			var dateRegex = new Regex (@"[0-9]{1,2}\/[0-9]{1,2}\/[0-9]{4}");
			var matches = dateRegex.Matches (entryDate.Text); 
			if (matches.Count == 2 &&
			    DateTime.TryParse (matches [0].Value, out start) && DateTime.TryParse (matches [1].Value, out end)) {

				startDate = start;
				EndDate = end;
			} else {
				UpdateEntryText ();
			}
		}

		#endregion

		bool ParseDates (ref DateTime start, ref DateTime end)
		{
			if (entryDate.Text == "") {
				startDate = null;
				endDate = null;
				return false;
			}

			//DateTime start, end;

			var dateRegex = new Regex (@"[0-9]{1,2}\/[0-9]{1,2}\/[0-9]{4}");
			var matches = dateRegex.Matches (entryDate.Text); 
			if (matches.Count == 2 &&
			    DateTime.TryParse (matches [0].Value, out start) && DateTime.TryParse (matches [1].Value, out end)) {

				//	startDate = start;
				//	EndDate = end;
			} else {
				UpdateEntryText ();
			}
			return true;
		}
	}
}

