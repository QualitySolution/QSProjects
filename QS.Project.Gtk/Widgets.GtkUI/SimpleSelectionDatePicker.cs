using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;
using Pango;

namespace QS.Widgets.GtkUI 
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class SimpleSelectionDatePicker : Gtk.Bin 
	{
		public static int? CalendarFontSize;

		public BindingControler<SimpleSelectionDatePicker> Binding { get; private set; }

		protected DateTime? date = null;
		public event EventHandler DateChanged;
		public event EventHandler DateChangedByUser;
		protected Gtk.Dialog editDate;

		public SimpleSelectionDatePicker() {
			this.Build();

			Binding = new BindingControler<SimpleSelectionDatePicker>(this, new Expression<Func<SimpleSelectionDatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});
		}

		public bool HideCalendarButton {
			get => !buttonEditDate.Visible;
			set => buttonEditDate.Visible = !value;
		}

		public string DateText {
			get => entryDate.Text;
		}

		public DateTime? DateOrNull {
			get => date;
			set {
				if(date == value)
					return;
				date = value;

				EntrySetDateTime(date);
				OnDateChanged();
			}
		}

		protected virtual void OnDateChanged() {
			Binding.FireChange(new Expression<Func<SimpleSelectionDatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});
			DateChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnDateChangedByUser() {
			DateChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		public DateTime Date {
			get => date.GetValueOrDefault();
			set {
				if(value == default(DateTime)) {
					DateOrNull = null;
				}
				else {
					DateOrNull = value;
				}
			}
		}

		public bool IsEmpty {
			get => !date.HasValue;

		}

		[DefaultValue(true)]
		public bool IsEditable {
			get => entryDate.IsEditable;
			set {
				entryDate.IsEditable = value;
				buttonEditDate.Sensitive = value;
			}
		}

		private bool _AutoSeparation = true;
		[DefaultValue(true)]
		public bool AutoSeparation {
			get => _AutoSeparation;
			set => _AutoSeparation = value;
		}

		protected void OnButtonEditDateClicked(object sender, EventArgs e) {
			Window parentWin = (Window)this.Toplevel;
			editDate = new Gtk.Dialog(
				"Укажите дату",
				parentWin,
				DialogFlags.DestroyWithParent
			) {
				HeightRequest = 260,
				WidthRequest = 200,
				Modal = true
			};		

			editDate.VBox.Add(GetButtonsVbox());

			editDate.AddButton("Отмена", ResponseType.Cancel);
			//editDate.AddButton("Ok", ResponseType.Ok);

			editDate.ShowAll();
			int response = editDate.Run();
			
			editDate.Destroy();
		}

		private VBox GetButtonsVbox() {
			int buttonsHeight = 150;
			int buttonsWidth = 30;

			Button todayButton = new Button() { HeightRequest = buttonsHeight, WidthRequest = buttonsWidth };
			todayButton.Label = $"Сегодня {DateTime.Now: dd.MM.yyyy}";
			todayButton.Clicked += (s, ev) => {
				SetTodayDate();
			};

			Button tommorowButton = new Button() { HeightRequest = buttonsHeight, WidthRequest = buttonsWidth };
			tommorowButton.Label = $"Завтра {DateTime.Now + TimeSpan.FromDays(1): dd.MM.yyyy}";
			tommorowButton.Clicked += (s, ev) => {
				SetTommorowDate();
			};

			Button otherDateButton = new Button() { HeightRequest = buttonsHeight, WidthRequest = buttonsWidth };
			otherDateButton.Label = $"Другая дата";
			otherDateButton.Clicked += (s, ev) => {
				SetOtherDate();
			};

			VBox vboxButtons = new VBox();
			vboxButtons.Add(todayButton);
			vboxButtons.Add(tommorowButton);
			vboxButtons.Add(otherDateButton);

			return vboxButtons;
		}

		private void SetTodayDate() {
			editDate.Destroy();
			DateOrNull = DateTime.Now.Date;
		}

		private void SetTommorowDate() {
			editDate.Destroy();
			DateOrNull = DateTime.Now.Date + TimeSpan.FromDays(1);
		}

		protected void SetOtherDate() {
			editDate.Destroy();

			Window parentWin = (Window)this.Toplevel;
			editDate = new Gtk.Dialog(
				"Укажите дату",
				parentWin,
				DialogFlags.DestroyWithParent) {
				Modal = true
			};

			editDate.AddButton("Отмена", ResponseType.Cancel);
			editDate.AddButton("Ok", ResponseType.Ok);

			Calendar SelectDate = new Calendar();
			SelectDate.DisplayOptions = CalendarDisplayOptions.ShowHeading |
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

			int response = editDate.Run();

			if(response == (int)ResponseType.Ok) {
				DateOrNull = SelectDate.GetDate();
				OnDateChangedByUser();
			}

			SelectDate.Destroy();
			editDate.Destroy();
		}

		public void Clear() {
			DateOrNull = null;
		}

		protected void OnEntryDateFocusInEvent(object o, FocusInEventArgs args) {
			entryDate.SelectRegion(0, 10);
		}

		protected void OnEntryDateFocusOutEvent(object o, FocusOutEventArgs args) {
			if(entryDate.Text == "") {
				DateOrNull = null;
				OnDateChangedByUser();
				return;
			}

			DateTime outDate;
			if(DateTime.TryParse(entryDate.Text, out outDate)) {
				DateOrNull = outDate;
				OnDateChangedByUser();
			}
			else {
				EntrySetDateTime(DateOrNull);
			}
		}

		void EntrySetDateTime(DateTime? date) {
			if(date.HasValue)
				entryDate.Text = date.Value.ToShortDateString();
			else
				entryDate.Text = String.Empty;
		}

		protected void OnEntryDateChanged(object sender, EventArgs e) {
			DateTime outDate;
			if(DateTime.TryParse(entryDate.Text, out outDate))
				entryDate.ModifyText(StateType.Normal);
			else
				entryDate.ModifyText(StateType.Normal, new Gdk.Color(255, 0, 0));
		}

		protected void OnCalendarDaySelectedDoubleClick(object sender, EventArgs e) {
			editDate.Respond(ResponseType.Ok);
		}

		protected void OnEntryDateTextInserted(object o, TextInsertedArgs args) {
			if(!_AutoSeparation) {
				return;
			}
			if(args.Length == 1 &&
			   (args.Position == 3 || args.Position == 6) &&
			   args.Text != System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator &&
			   args.Position == entryDate.Text.Length) 
			   {
				int Pos = args.Position - 1;
				entryDate.InsertText(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, ref Pos);
				args.Position++;
			}
		}

		protected void OnEntryDateActivated(object sender, EventArgs e) {
			this.ChildFocus(DirectionType.TabForward);
		}

		public new void ModifyBase(StateType state, Gdk.Color color) {
			entryDate.ModifyBase(state, color);
		}
	}
}
