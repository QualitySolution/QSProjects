using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;
using Gtk;
using Pango;
using QS.Extensions;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem (true)]
	[Category("QS.Project")]
	public partial class DateRangePicker : Bin
	{
		public static int? CalendarFontSize;

		public BindingControler<DateRangePicker> Binding { get; private set; }

		#region Fields

		Label periodSummary;
		Calendar StartDateCalendar, EndDateCalendar;
		DatePicker StartDateEntry, EndDateEntry;
		bool skipPeriodChangedEvent;

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

		/// <summary>
		/// Если не заполнена начальная и конечная дата при открытии датапикера ставить текущую дату. По умолчанию - true
		/// </summary>
		public bool SetCurrentDateByDefault = true;

		CalendarDisplayOptions DisplayOptions = CalendarDisplayOptions.ShowHeading | CalendarDisplayOptions.ShowDayNames;

		#endregion

		#region Events

		public event EventHandler PeriodChanged;
		public event EventHandler PeriodChangedByUser;
		public event EventHandler StartDateChanged;
		public event EventHandler EndDateChanged;

		protected virtual void OnPeriodChanged ()
		{
			if (skipPeriodChangedEvent)
				return;
			UpdateEntryText ();
			PeriodChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnPeriodChangedByUser()
		{
			PeriodChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnStartDateChanged ()
		{
			Binding.FireChange(new Expression<Func<DateRangePicker, object>>[] {
				(w => w.StartDate),
				(w => w.StartDateOrNull),
			});

			StartDateChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnEndDateChanged ()
		{
			Binding.FireChange(new Expression<Func<DateRangePicker, object>>[] {
				(w => w.EndDate),
				(w => w.EndDateOrNull),
			});

			EndDateChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		public DateRangePicker()
		{
			this.Build ();

			Binding = new BindingControler<DateRangePicker>(this, new Expression<Func<DateRangePicker, object>>[] {
				(w => w.StartDate),
				(w => w.StartDateOrNull),
				(w => w.EndDate),
				(w => w.EndDateOrNull)
			});

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
				entryDate.Text = String.Format ("Начиная с {0:d}", StartDate);
			else if(EndDateOrNull.HasValue)
				entryDate.Text = String.Format ("До {0:d}", EndDate);
		}

		#region Event handlers

		#region Всплывающий диалог

		protected void OnButtonPickDatePeriodClicked (object sender, EventArgs e)
		{
			#region Widget creation
			Window parentWin = (Window)Toplevel;
			var selectDate = new Gtk.Dialog ("Укажите период", parentWin, DialogFlags.DestroyWithParent);
			selectDate.Modal = true;
			selectDate.AddButton ("Отмена", ResponseType.Cancel);
			selectDate.AddButton ("Ok", ResponseType.Ok);

			periodSummary = new Label();
			selectDate.VBox.Add(periodSummary);

			HBox hbox = new HBox (true, 6);
			var startVbox = new VBox(false, 3);
			var endVbox = new VBox(false, 3);

			StartDateCalendar = new Calendar ();
			StartDateCalendar.DisplayOptions = DisplayOptions;
			StartDateCalendar.DaySelected += StartDateCalendar_DaySelected;
			StartDateCalendar.MonthChanged += StartDateCalendar_MonthChanged;
			StartDateCalendar.Day = 0;
			StartDateCalendar_MonthChanged(null, null);

			EndDateCalendar = new Calendar ();
			EndDateCalendar.DisplayOptions = DisplayOptions;
			EndDateCalendar.DaySelected += EndDateCalendar_DaySelected;
			EndDateCalendar.MonthChanged += EndDateCalendar_MonthChanged;
			EndDateCalendar.Day = 0;
			EndDateCalendar_MonthChanged(null, null);

			if(CalendarFontSize.HasValue) {
				var desc = new FontDescription { AbsoluteSize = CalendarFontSize.Value * 1000 };
				StartDateCalendar.ModifyFont(desc);
				EndDateCalendar.ModifyFont(desc);
			}

			StartDateEntry = new DatePicker();
			StartDateEntry.DateChanged += StartDateEntry_DateChanged;

			EndDateEntry = new DatePicker();
			EndDateEntry.DateChanged += EndDateEntry_DateChanged;

			startVbox.Add(StartDateCalendar);
			startVbox.Add(StartDateEntry);
			endVbox.Add(EndDateCalendar);
			endVbox.Add(EndDateEntry);

			hbox.Add(startVbox);
			hbox.Add(endVbox);

			selectDate.VBox.Add (hbox);
			selectDate.ShowAll ();

			StartDateEntry.HideCalendarButton = true;
			EndDateEntry.HideCalendarButton = true;

			if(SetCurrentDateByDefault && !StartDateOrNull.HasValue && !EndDateOrNull.HasValue) {
				StartDateEntry.DateOrNull = DateTime.Today.Date;
				EndDateEntry.DateOrNull = DateTime.Today.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
			} else {
				StartDateEntry.DateOrNull = StartDateOrNull;
				EndDateEntry.DateOrNull = EndDateOrNull;
			}
			#endregion

			int response = selectDate.Run ();
			if (response == (int)ResponseType.Ok) {
				startDate = StartDateEntry.DateOrNull;
				endDate = EndDateEntry.DateOrNull;
				OnStartDateChanged();
				OnEndDateChanged();
				OnPeriodChanged ();
				OnPeriodChangedByUser();
			}

			#region Destroy
			EndDateCalendar.Destroy ();
			StartDateCalendar.Destroy ();
			StartDateEntry.Destroy();
			EndDateEntry.Destroy();
			startVbox.Destroy();
			endVbox.Destroy();
			hbox.Destroy ();
			selectDate.Destroy ();
			#endregion
		}

		void StartDateCalendar_MonthChanged(object sender, EventArgs e)
		{
			if(StartDateCalendar.Date.Month == DateTime.Today.Month && StartDateCalendar.Date.Year == DateTime.Today.Year)
				StartDateCalendar.MarkDay((uint)DateTime.Today.Day);
			else
				StartDateCalendar.UnmarkDay((uint)DateTime.Today.Day);
		}

		void EndDateCalendar_MonthChanged(object sender, EventArgs e)
		{
			if(EndDateCalendar.Date.Month == DateTime.Today.Month && EndDateCalendar.Date.Year == DateTime.Today.Year)
				EndDateCalendar.MarkDay((uint)DateTime.Today.Day);
			else
				EndDateCalendar.UnmarkDay((uint)DateTime.Today.Day);
		}

		void StartDateEntry_DateChanged(object sender, EventArgs e)
		{
			if(StartDateEntry.IsEmpty) {
				StartDateCalendar.Day = 0;
			} else {
				StartDateCalendar.Date = StartDateEntry.Date;
			}
			UpdatePeriodInDialog();
		}

		void EndDateEntry_DateChanged(object sender, EventArgs e)
		{
			if(EndDateEntry.IsEmpty) {
				EndDateCalendar.Day = 0;
			} else {
				EndDateCalendar.Date = EndDateEntry.Date;
			}
			UpdatePeriodInDialog();
		}

		void EndDateCalendar_DaySelected (object sender, EventArgs e)
		{
			if(EndDateCalendar.Day != 0 && EndDateCalendar.Date != EndDateEntry.Date)
				EndDateEntry.Date = EndDateCalendar.Date;
		}

		void StartDateCalendar_DaySelected (object sender, EventArgs e)
		{
			if(StartDateCalendar.Day != 0 && StartDateCalendar.Date != StartDateEntry.Date)
				StartDateEntry.Date = StartDateCalendar.Date;
		}

		void UpdatePeriodInDialog()
		{
			string text;
			if(StartDateEntry.IsEmpty && EndDateEntry.IsEmpty)
				text = String.Empty;
			else if(EndDateEntry.IsEmpty)
				text = String.Format("начиная с {0:d}", StartDateEntry.Date);
			else if(StartDateEntry.IsEmpty)
				text = String.Format("до {0:d}", EndDateEntry.Date);
			else if(StartDateEntry.Date.Year != EndDateEntry.Date.Year)
				text = String.Format("{0:D} - {1:D}", StartDateEntry.Date, EndDateEntry.Date);
			else if(StartDateEntry.Date.Month != EndDateEntry.Date.Month)
				text = String.Format("{0:dd MMMMM}-{1:D}", StartDateEntry.Date, EndDateEntry.Date);
			else if(StartDateEntry.Date.Day != EndDateEntry.Date.Day)
				text = String.Format("{0:dd}-{1:D}", StartDateEntry.Date, EndDateEntry.Date);
			else
				text = String.Format("{0:D}", StartDateEntry.Date);

			if (StartDateEntry.IsEmpty || EndDateEntry.IsEmpty || StartDateEntry.Date <= EndDateEntry.Date)
				periodSummary.Markup = text;
			else
				periodSummary.Markup = String.Format ("<span foreground=\"red\">{0}</span>", text);
		}

		#endregion

		protected void OnEntryDateFocusOutEvent (object o, FocusOutEventArgs args)
		{
			DateTime? startParsed, endParsed;
			if(ParseDates(entryDate.Text, out startParsed, out endParsed)) {
				skipPeriodChangedEvent = true;
				StartDateOrNull = startParsed;
				EndDateOrNull = endParsed;
				skipPeriodChangedEvent = false;
				OnPeriodChanged();
				OnPeriodChangedByUser();
			}
			UpdateEntryText();
		}

		#endregion

		#region public Methods

		/// <summary>
		/// Используется для одновременной установки начала и конца периода, с вызовом всего одного события PeriodChanged
		/// </summary>
		/// <param name="start">Начало периода</param>
		/// <param name="end">Конец периода</param>
		public void SetPeriod(DateTime? start, DateTime? end)
		{
			skipPeriodChangedEvent = true;
			StartDateOrNull = start;
			EndDateOrNull = end;
			skipPeriodChangedEvent = false;
			OnPeriodChanged();
		}

		#endregion

		static bool ParseDates (string textRange, out DateTime? start, out DateTime? end)
		{
			if (String.IsNullOrWhiteSpace(textRange)) {
				start = null;
				end = null;
				return true;
			}

			DateTime startTemp, endTemp;
			var dateRegex = new Regex(@"[0-9]{1,2}(\/|\.|\-|\\)[0-9]{1,2}(\/|\.|\-|\\)[0-9]{2,4}");
			var matches = dateRegex.Matches(textRange);
			if(matches.Count == 2 && DateTime.TryParse(matches[0].Value, out startTemp) && DateTime.TryParse(matches[1].Value, out endTemp)) {
				start = startTemp;
				end = endTemp;
				return true;
			} 
			else if(matches.Count == 1 && DateTime.TryParse(matches[0].Value, out startTemp)) {
				start = startTemp;
				end = startTemp;
				return true;
			}

			start = null;
			end = null;
			return false;
		}

		protected void OnEntryDateChanged(object sender, EventArgs e)
		{
			DateTime? startParsed, endParsed;
			if(ParseDates(entryDate.Text, out startParsed, out endParsed)) {
				if(startParsed != null && endParsed != null && startParsed > endParsed)
					entryDate.ModifyText(StateType.Normal, new Gdk.Color(255, 165, 0));
				else
					entryDate.ModifyText(StateType.Normal);
			}
			else
				entryDate.ModifyText(StateType.Normal, new Gdk.Color(255, 0, 0));
		}

		protected override void OnDestroyed() {
			var image = buttonPickDatePeriod.Image as Image;
			image.DisposeImagePixbuf();
			Binding.CleanSources();
			base.OnDestroyed();
		}

		protected void OnButtonClearDateClicked(object sender, EventArgs e) {
			Clear();
		}

		public void Clear() {
			StartDateOrNull = null;
			EndDateOrNull = null;
		}
	}
}

