using System;
using System.Collections.Generic;
using Gtk;

namespace QSWidgetLib
{
	public enum RadioModeType { SingleColumn, DoubleColumn };

	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelectPeriod : Gtk.Bin
	{
		private Dictionary<Period, bool> ShowRadio;
		private Dictionary<Period, RadioButton> RadioButtons;
		private RadioModeType _RadioMode;
		private bool IsRadioChange;
		private int customPeriodInDays;
		public event EventHandler DatesChanged;
		public event Action<bool> EarlyCustomDateToggled;

		public enum Period { None, Today, Week, Month, ThreeMonth, SixMonth, Year, AllTime, CurWeek, CurMonth, CurQuarter, CurYear, CustomPeriod };

		public RadioModeType RadioMode {
			get => _RadioMode;
			set {
				_RadioMode = value;
				OnRepackRadios();
			}
		}

		public DateTime DateBegin {
			get => StartDate.Date;
			set => StartDate.Date = value;
		}

		public DateTime DateEnd {
			get => EndDate.Date;
			set => EndDate.Date = value;
		}

		public bool AutoDateSeparation {
			get => StartDate.AutoSeparation;
			set {
				EndDate.AutoSeparation = value;
				StartDate.AutoSeparation = value;
			}
		}

		public bool IsAllTime => StartDate.IsEmpty && EndDate.IsEmpty;

		public bool ShowToday {
			get => ShowRadio[Period.Today];
			set {
				ShowRadio[Period.Today] = value;
				OnRepackRadios();
			}
		}

		public bool ShowWeek {
			get => ShowRadio[Period.Week];
			set {
				ShowRadio[Period.Week] = value;
				OnRepackRadios();
			}
		}

		public bool ShowMonth {
			get => ShowRadio[Period.Month];
			set {
				ShowRadio[Period.Month] = value;
				OnRepackRadios();
			}
		}

		public bool Show3Month {
			get => ShowRadio[Period.ThreeMonth];
			set {
				ShowRadio[Period.ThreeMonth] = value;
				OnRepackRadios();
			}
		}

		public bool Show6Month {
			get => ShowRadio[Period.SixMonth];
			set {
				ShowRadio[Period.SixMonth] = value;
				OnRepackRadios();
			}
		}

		public bool ShowYear {
			get => ShowRadio[Period.Year];
			set {
				ShowRadio[Period.Year] = value;
				OnRepackRadios();
			}
		}

		public bool ShowAllTime {
			get => ShowRadio[Period.AllTime];
			set {
				ShowRadio[Period.AllTime] = value;
				OnRepackRadios();
			}
		}

		public bool ShowCurWeek {
			get => ShowRadio[Period.CurWeek];
			set {
				ShowRadio[Period.CurWeek] = value;
				OnRepackRadios();
			}
		}

		public bool ShowCurMonth {
			get => ShowRadio[Period.CurMonth];
			set {
				ShowRadio[Period.CurMonth] = value;
				OnRepackRadios();
			}
		}

		public bool ShowCurQuarter {
			get => ShowRadio[Period.CurQuarter];
			set {
				ShowRadio[Period.CurQuarter] = value;
				OnRepackRadios();
			}
		}

		public bool ShowCurYear {
			get => ShowRadio[Period.CurYear];
			set {
				ShowRadio[Period.CurYear] = value;
				OnRepackRadios();
			}
		}

		public bool ShowCustomPeriod {
			get => ShowRadio[Period.CustomPeriod];
			set {
				if(ShowRadio.ContainsKey(Period.CustomPeriod)) {
					ShowRadio[Period.CustomPeriod] = value;
					OnRepackRadios();
				}
			}
		}

		public bool WithTime {
			get => EndDate.WithTime || StartDate.WithTime;
			set => EndDate.WithTime = StartDate.WithTime = value;
		}

		public SelectPeriod()
		{
			this.Build();
			EndDate.WithTime = StartDate.WithTime = WithTime;
			//Создаем переключатели
			RadioButtons = new Dictionary<Period, RadioButton>();
			RadioButtons.Add(Period.Today, new RadioButton("Сегодня"));
			RadioButtons[Period.Today].Clicked += OnRadioTodayClicked;
			RadioButton RadioToday = RadioButtons[Period.Today];
			RadioButtons.Add(Period.Week, new RadioButton(RadioToday, "За неделю"));
			RadioButtons[Period.Week].Clicked += OnRadioWeekClicked;
			RadioButtons.Add(Period.Month, new RadioButton(RadioToday, "За месяц"));
			RadioButtons[Period.Month].Clicked += OnRadioMonthClicked;
			RadioButtons.Add(Period.ThreeMonth, new RadioButton(RadioToday, "За 3 месяца"));
			RadioButtons[Period.ThreeMonth].Clicked += OnRadio3monthClicked;
			RadioButtons.Add(Period.SixMonth, new RadioButton(RadioToday, "За полгода"));
			RadioButtons[Period.SixMonth].Clicked += OnRadio6monthClicked;
			RadioButtons.Add(Period.Year, new RadioButton(RadioToday, "За год"));
			RadioButtons[Period.Year].Clicked += OnRadioYearClicked;
			RadioButtons.Add(Period.AllTime, new RadioButton(RadioToday, "Все время"));
			RadioButtons[Period.AllTime].Clicked += OnRadioAllClicked;

			RadioButtons.Add(Period.CurWeek, new RadioButton(RadioToday, "Тек. неделя"));
			RadioButtons[Period.CurWeek].Clicked += OnRadioCurWeekClicked;
			RadioButtons.Add(Period.CurMonth, new RadioButton(RadioToday, "Тек. месяц"));
			RadioButtons[Period.CurMonth].Clicked += OnRadioCurMonthClicked;
			RadioButtons.Add(Period.CurQuarter, new RadioButton(RadioToday, "Тек. квартал"));
			RadioButtons[Period.CurQuarter].Clicked += OnRadioCurQuarterClicked;
			RadioButtons.Add(Period.CurYear, new RadioButton(RadioToday, "Тек. год"));
			RadioButtons[Period.CurYear].Clicked += OnRadioCurYearClicked;

			ShowRadio = new Dictionary<Period, bool>();
			ShowRadio.Add(Period.Today, true);
			ShowRadio.Add(Period.Week, true);
			ShowRadio.Add(Period.Month, true);
			ShowRadio.Add(Period.ThreeMonth, false);
			ShowRadio.Add(Period.SixMonth, false);
			ShowRadio.Add(Period.Year, false);
			ShowRadio.Add(Period.CurWeek, false);
			ShowRadio.Add(Period.CurMonth, false);
			ShowRadio.Add(Period.CurQuarter, false);
			ShowRadio.Add(Period.CurYear, false);
			ShowRadio.Add(Period.AllTime, true);

			_RadioMode = RadioModeType.DoubleColumn;
			OnRepackRadios();
			ActiveRadio = Period.AllTime;
			chkEarlyCustomDate.Toggled += OnChkEarlyCustomDateToggled;
		}

		void OnChkEarlyCustomDateToggled(object sender, EventArgs e) {
			var radBtnToday = RadioButtons[Period.Today];

			if(chkEarlyCustomDate.Active) {
				radBtnToday.Label = "Последний день";
			}
			else {
				radBtnToday.Label = "Сегодня";
			}

			EarlyCustomDateToggled?.Invoke(chkEarlyCustomDate.Active);

			if(ActiveRadio != Period.None) {
				RadioButtons[ActiveRadio].Click();
			}
		}

		public void AddCustomPeriodInDays(int periodInDays, string chkEarlyCustomDateName = null) {
			chkEarlyCustomDate.Visible = true;
			chkEarlyCustomDate.Sensitive = true;
			customPeriodInDays = periodInDays;

			if(!string.IsNullOrEmpty(chkEarlyCustomDateName)) {
				chkEarlyCustomDate.Label = chkEarlyCustomDateName;
			}

			var RadioToday = RadioButtons[Period.Today];
			RadioButtons.Add(Period.CustomPeriod, new RadioButton(RadioToday, $"{customPeriodInDays} дн."));
			RadioButtons[Period.CustomPeriod].Clicked += OnRadioCustomPeriodClicked;
			ShowRadio.Add(Period.CustomPeriod, false);
		}

		public Period ActiveRadio {
			get {
				foreach(KeyValuePair<Period, RadioButton> Pair in RadioButtons) {
					if(Pair.Value.Active)
						return Pair.Key;
				};
				return Period.None;
			}
			set {
				if(value != Period.None)
					RadioButtons[value].Click();
			}
		}

		protected void OnRadioTodayClicked(object sender, EventArgs e) {
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;

			if(chkEarlyCustomDate.Active) {
				StartDate.Date = DateTime.Today.AddDays(-customPeriodInDays);
				EndDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1).AddTicks(-1);
			}
			else {
				StartDate.Date = DateTime.Today.Date;
				EndDate.Date = DateTime.Today.AddDays(1).AddTicks(-1);
			}
			EndRadioChange();
		}

		protected void OnRadioWeekClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;

			if(chkEarlyCustomDate.Active) {
				StartDate.Date = DateTime.Today.AddDays(-customPeriodInDays - 7);
				EndDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1).AddTicks(-1);
			}
			else {
				StartDate.Date = DateTime.Today.AddDays(-7);
				EndDate.Date = DateTime.Today.AddDays(1).AddTicks(-1);
			}
			EndRadioChange();
		}

		protected void OnRadioMonthClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;

			if(chkEarlyCustomDate.Active) {
				StartDate.Date = DateTime.Today.AddDays(-customPeriodInDays).AddMonths(-1);
				EndDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1).AddTicks(-1);
			}
			else {
				StartDate.Date = DateTime.Today.AddMonths(-1);
				EndDate.Date = DateTime.Today.AddDays(1).AddTicks(-1);
			}
			EndRadioChange();
		}

		protected void OnRadio3monthClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;

			if(chkEarlyCustomDate.Active) {
				StartDate.Date = DateTime.Today.AddDays(-customPeriodInDays).AddMonths(-3);
				EndDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1).AddTicks(-1);
			}
			else {
				StartDate.Date = DateTime.Today.AddMonths(-3);
				EndDate.Date = DateTime.Today.AddDays(1).AddTicks(-1);
			}
			EndRadioChange();
		}

		protected void OnRadio6monthClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;

			if(chkEarlyCustomDate.Active) {
				StartDate.Date = DateTime.Today.AddDays(-customPeriodInDays).AddMonths(-6);
				EndDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1).AddTicks(-1);
			}
			else {
				StartDate.Date = DateTime.Today.AddMonths(-6);
				EndDate.Date = DateTime.Today.AddDays(1).AddTicks(-1);
			}
			EndRadioChange();
		}

		protected void OnRadioYearClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;

			if(chkEarlyCustomDate.Active) {
				StartDate.Date = DateTime.Today.AddDays(-customPeriodInDays).AddYears(-1);
				EndDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1).AddTicks(-1);
			}
			else {
				StartDate.Date = DateTime.Today.AddYears(-1);
				EndDate.Date = DateTime.Today.AddDays(1).AddTicks(-1);
			}
			EndRadioChange();
		}

		protected void OnRadioAllClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;
			StartDate.Date = new DateTime(1, 1, 1);
			EndDate.Date = new DateTime(1, 1, 1);
			EndRadioChange();
		}

		protected void OnRadioCurWeekClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;
			int Day;
			if(DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
				Day = 7;
			else
				Day = (int)DateTime.Now.DayOfWeek;
			StartDate.Date = DateTime.Today.AddDays(1 - Day);
			EndDate.Date = DateTime.Now.AddDays(7 - Day);
			EndRadioChange();
		}

		protected void OnRadioCurMonthClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;
			int Year = DateTime.Today.Year;
			int Month = DateTime.Today.Month;
			System.Globalization.Calendar cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
			int LastDay = cal.GetDaysInMonth(Year, Month);
			StartDate.Date = new DateTime(Year, Month, 1);
			EndDate.Date = new DateTime(Year, Month, LastDay);
			EndRadioChange();
		}

		protected void OnRadioCurQuarterClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;
			int quarterNumber = (DateTime.Today.Month - 1) / 3 + 1;
			StartDate.Date = new DateTime(DateTime.Today.Year, (quarterNumber - 1) * 3 + 1, 1);
			EndDate.Date = StartDate.Date.AddMonths(3).AddDays(-1);
			EndRadioChange();
		}

		protected void OnRadioCurYearClicked(object sender, EventArgs e)
		{
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;
			StartDate.Date = new DateTime(DateTime.Today.Year, 1, 1);
			EndDate.Date = new DateTime(DateTime.Today.Year, 12, 31);
			EndRadioChange();
		}

		protected void OnRadioCustomPeriodClicked(object sender, EventArgs e) {
			if((sender as RadioButton).Active == false)
				return;
			IsRadioChange = true;
			if(chkEarlyCustomDate.Active) {
				StartDate.Date = DateTime.Today.AddDays(- 2 * customPeriodInDays + 1);
				EndDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1).AddTicks(-1);
			}
			else {
				StartDate.Date = DateTime.Today.AddDays(-customPeriodInDays + 1);
				EndDate.Date = DateTime.Today.AddDays(1).AddTicks(-1);
			}
			EndRadioChange();
		}

		protected void OnRepackRadios()
		{
			uint Count = 0;
			foreach(KeyValuePair<Period, bool> pair in ShowRadio) {
				if(pair.Value)
					Count++;
			}
			uint MaxRow, MaxCol;
			if(_RadioMode == RadioModeType.DoubleColumn) {
				MaxCol = 2;
				MaxRow = (Count / 2) + (Count % 2);
			} else {
				MaxCol = 1;
				MaxRow = Count;
			}
			foreach(Widget item in RadiosTable.AllChildren) {
				RadiosTable.Remove(item);
			}

			RadiosTable.NRows = MaxRow;
			RadiosTable.NColumns = MaxCol;
			uint CurRow = 0;
			uint CurCol = 0;
			foreach(KeyValuePair<Period, bool> pair in ShowRadio) {
				if(pair.Value) {
					RadiosTable.Attach(RadioButtons[pair.Key], CurCol, CurCol + 1, CurRow, CurRow + 1);
					CurRow++;
					if(_RadioMode == RadioModeType.DoubleColumn && MaxRow <= CurRow) {
						CurRow = 0;
						CurCol++;
					}
				}
			}
			RadiosTable.ShowAll();
		}

		protected void OnStartDateDateChanged(object sender, EventArgs e)
		{
			if(DatesChanged != null && !IsRadioChange)
				DatesChanged(this, EventArgs.Empty);
		}

		protected void OnEndDateDateChanged(object sender, EventArgs e)
		{
			if(DatesChanged != null && !IsRadioChange)
				DatesChanged(this, EventArgs.Empty);
		}

		private void EndRadioChange()
		{
			IsRadioChange = false;
			DatesChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}

