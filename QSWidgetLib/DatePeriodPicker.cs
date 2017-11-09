using System;
using Gtk;
using System.Text.RegularExpressions;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DatePeriodPicker : Bin
	{
		#region Fields

		Label periodSummary;
		Calendar StartDateCalendar, EndDateCalendar;

		private DateTime? startDate;

		public DateTime? StartDateOrNull {
			get {
				return startDate;
			}
			set {
				if(value != startDate)
				{
					startDate = value;
					OnStartDateChanged();
					OnPeriodChanged ();
				}
			}
		}

		public DateTime StartDate { 
			get { return startDate.HasValue ? startDate.Value : default(DateTime); }
			set { 
				if (value != startDate) {
					startDate = value != default(DateTime) ? (DateTime?)value : null;
					OnStartDateChanged();
					OnPeriodChanged ();
				}
			}
		}

		private DateTime? endDate = null;

		public DateTime? EndDateOrNull {
			get {
				return endDate;
			}
			set {
				if(value != endDate)
				{
					endDate = value;
					OnEndDateChanged();
					OnPeriodChanged ();
				}
			}
		}

		public DateTime EndDate { 
			get { return endDate.HasValue ? endDate.Value : default(DateTime); }
			set { 
				if (value != endDate) {
					endDate = value != default(DateTime) ? (DateTime?)value : null;
					OnEndDateChanged();
					OnPeriodChanged ();
				}
			}
		}

		CalendarDisplayOptions DisplayOptions = CalendarDisplayOptions.ShowHeading | CalendarDisplayOptions.ShowDayNames;

		#endregion

		#region Events

		public event EventHandler PeriodChanged;
		public event EventHandler PeriodChangedByUser;
		public event EventHandler StartDateChanged;
		public event EventHandler EndDateChanged;

		protected virtual void OnPeriodChanged ()
		{
			UpdateEntryText ();
			PeriodChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnPeriodChangedByUser()
		{
			PeriodChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnStartDateChanged ()
		{
			StartDateChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnEndDateChanged ()
		{
			EndDateChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		public DatePeriodPicker ()
		{
			this.Build ();
			UpdateEntryText ();
		}

		private void  UpdateEntryText ()
		{
			if (StartDateOrNull.HasValue && EndDateOrNull.HasValue)
			{
				if(StartDateOrNull.Value == EndDateOrNull.Value)
					entryDate.Text = String.Format ("{0:d}", StartDate);
				else
					entryDate.Text = String.Format ("{0:d} - {1:d}", StartDate, EndDate);
			}
			else if (!StartDateOrNull.HasValue && !EndDateOrNull.HasValue)
				entryDate.Text = String.Empty;
			else if(StartDateOrNull.HasValue)
				entryDate.Text = String.Format ("начиная с {0:d}", StartDate);
			else if(EndDateOrNull.HasValue)
				entryDate.Text = String.Format ("до {0:d}", EndDate);
		}

		#region Event handlers

		protected void OnButtonPickDatePeriodClicked (object sender, EventArgs e)
		{
			#region Widget creation
			Window parentWin = (Window)Toplevel;
			var selectDate = new Dialog ("Укажите период", parentWin, DialogFlags.DestroyWithParent);
			selectDate.Modal = true;
			selectDate.AddButton ("Отмена", ResponseType.Cancel);
			selectDate.AddButton ("Ok", ResponseType.Ok);

			periodSummary = new Label();
			selectDate.VBox.Add(periodSummary);

			HBox hbox = new HBox (true, 6);

			StartDateCalendar = new Calendar ();
			StartDateCalendar.DisplayOptions = DisplayOptions;
			StartDateCalendar.Date = StartDateOrNull ?? DateTime.Today;
			StartDateCalendar.DaySelected += StartDateCalendar_DaySelected;

			EndDateCalendar = new Calendar ();
			EndDateCalendar.DisplayOptions = DisplayOptions;
			EndDateCalendar.Date = EndDateOrNull ?? DateTime.Today;
			EndDateCalendar.DaySelected += EndDateCalendar_DaySelected;

			hbox.Add (StartDateCalendar);
			hbox.Add (EndDateCalendar);

			selectDate.VBox.Add (hbox);
			selectDate.ShowAll ();
			#endregion

			int response = selectDate.Run ();
			if (response == (int)ResponseType.Ok) {
				startDate = StartDateCalendar.GetDate ();
				endDate = EndDateCalendar.GetDate ();
				OnStartDateChanged();
				OnEndDateChanged();
				OnPeriodChanged ();
				OnPeriodChangedByUser();
			}

			#region Destroy
			EndDateCalendar.Destroy ();
			StartDateCalendar.Destroy ();
			hbox.Destroy ();
			selectDate.Destroy ();
			#endregion
		}

		void EndDateCalendar_DaySelected (object sender, EventArgs e)
		{
			UpdatePeriodInDialog ();
		}

		void StartDateCalendar_DaySelected (object sender, EventArgs e)
		{
			UpdatePeriodInDialog ();
		}

		void UpdatePeriodInDialog()
		{
			string text;
			if(StartDateCalendar.Date.Year != EndDateCalendar.Date.Year)
				text = String.Format("{0:D} - {1:D}", StartDateCalendar.Date, EndDateCalendar.Date);
			else if(StartDateCalendar.Date.Month != EndDateCalendar.Date.Month)
				text = String.Format("{0:dd MMMMM}-{1:D}", StartDateCalendar.Date, EndDateCalendar.Date);
			else if(StartDateCalendar.Date.Day != EndDateCalendar.Date.Day)
				text = String.Format("{0:dd}-{1:D}", StartDateCalendar.Date, EndDateCalendar.Date);
			else
				text = String.Format("{0:D}", StartDateCalendar.Date);

			if (StartDateCalendar.Date <= EndDateCalendar.Date)
				periodSummary.Markup = text;
			else
				periodSummary.Markup = String.Format ("<span foreground=\"red\">{0}</span>", text);
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

			var dateRegex = new Regex (@"[0-9]{1,2}\/[0-9]{1,2}\/[0-9]{4}");
			var matches = dateRegex.Matches (entryDate.Text); 
			if (matches.Count == 2 &&
			    DateTime.TryParse (matches [0].Value, out start) && DateTime.TryParse (matches [1].Value, out end)) {

			} else {
				UpdateEntryText ();
			}
			return true;
		}

		[GLib.ConnectBefore]
		protected void OnEntryDateKeyPressEvent (object o, KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Delete || args.Event.Key == Gdk.Key.BackSpace)
			{
				endDate = null; // Что бы событие вызывалось 1 раз.
				StartDateOrNull = null;
				OnPeriodChangedByUser();
			}
		}
	}
}

