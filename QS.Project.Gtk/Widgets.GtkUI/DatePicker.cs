using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;
using Pango;
using QS.Extensions;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class DatePicker : Gtk.Bin
	{
		public static int? CalendarFontSize;

		public BindingControler<DatePicker> Binding { get; private set; }

		protected DateTime? date = null;
		public event EventHandler DateChanged;
		public event EventHandler DateChangedByUser;
		protected Gtk.Dialog editDate;

		public DatePicker ()
		{
			this.Build ();

			Binding = new BindingControler<DatePicker>(this, new Expression<Func<DatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});
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

		public bool HideCalendarButton
		{
			get => !buttonEditDate.Visible;
			set => buttonEditDate.Visible = !value;
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
			Binding.FireChange(new Expression<Func<DatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});
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
				buttonEditDate.Sensitive = value;
				buttonClearDate.Sensitive = value;}
		}

		private bool _AutoSeparation = true;
		[DefaultValue(true)]
		public bool AutoSeparation{
			get { return _AutoSeparation;}
			set { _AutoSeparation = value;}
		}

		protected void OnButtonEditDateClicked (object sender, EventArgs e)
		{
			Window parentWin = (Window) this.Toplevel;
			editDate = new Gtk.Dialog(
				WithTime ? "Укажите дату и время" : "Укажите дату",
				parentWin,
				DialogFlags.DestroyWithParent
			) {
				Modal = true
			};
			editDate.AddButton ("Отмена", ResponseType.Cancel);
			editDate.AddButton ("Ok", ResponseType.Ok);
			TimeEntry timeEntry = null;
			if(WithTime) {
				Label timeLabel = new Label("Время:");
				timeEntry = new TimeEntry {
					DateTime = date ?? DateTime.Today,
					AutocompleteStep = 5
				};
				HScale timeScale = new HScale(0, 1439, 5) {
					DrawValue = false,
					Value = Date.Subtract(Date.Date).TotalMinutes
				};
				timeScale.Adjustment.PageIncrement = 60;
				timeScale.ValueChanged += (o, args) => {
					if(!timeEntry.HasFocus)
						timeEntry.Time = TimeSpan.FromMinutes(timeScale.Value);
				};
				timeEntry.Changed += (s, ea) => timeScale.Value = timeEntry.Time.TotalMinutes;
				VBox timeCtrlBox = new VBox {
					timeEntry,
					timeScale
				};
				HBox timeBox = new HBox {
					timeLabel,
					timeCtrlBox
				};
				editDate.VBox.Add(timeBox);
			}
			Calendar SelectDate = new Calendar ();
			SelectDate.DisplayOptions = CalendarDisplayOptions.ShowHeading  | 
				CalendarDisplayOptions.ShowDayNames | 
					CalendarDisplayOptions.ShowWeekNumbers;
			SelectDate.DaySelectedDoubleClick += OnCalendarDaySelectedDoubleClick;
			SelectDate.Date = date ?? DateTime.Now.Date;

			if(CalendarFontSize.HasValue) {
				var desc = new FontDescription { AbsoluteSize = CalendarFontSize.Value * 1000 };
				SelectDate.ModifyFont(desc);
			}
			editDate.VBox.Add(SelectDate);
			editDate.ShowAll();
			int response = editDate.Run ();
			DateTime tempDate = WithTime ? SelectDate.GetDate() + timeEntry.Time : SelectDate.GetDate();
			SelectDate.Destroy();
			editDate.Destroy ();
			if(response == (int)ResponseType.Ok) {
				DateOrNull = tempDate;
				OnDateChangedByUser();
			}
		}

		public void Clear()
		{
			DateOrNull = null;
		}


		[Browsable(true)]
		public bool HideButtonClearDate {
			get =>  !buttonClearDate.Visible;
			set => buttonClearDate.Visible = !value;
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

		public new void ModifyBase(StateType state, Gdk.Color color){
			entryDate.ModifyBase(state, color);
		}

		protected void OnButtonClearDateClicked(object sender, EventArgs e) 
		{
			this.Clear();
		}

		protected override void OnDestroyed() {
			var clearImage = buttonClearDate.Image as Image;
			clearImage.DisposeImagePixbuf();
			var editImage = buttonEditDate.Image as Image;
			editImage.DisposeImagePixbuf();
			base.OnDestroyed();
		}
	}
}

