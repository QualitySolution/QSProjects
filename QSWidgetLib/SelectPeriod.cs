using System;
using System.Collections.Generic;
using Gtk;

namespace QSWidgetLib
{
	public enum RadioModeType{SingleColumn, DoubleColumn};

	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelectPeriod : Gtk.Bin
	{
		private Dictionary<Period, bool> ShowRadio;
		private Dictionary<Period, RadioButton> RadioButtons;
		private RadioModeType _RadioMode;
		private bool UsingRadio;
		public event EventHandler DatesChanged;

		public enum Period {None, Today, Week, Month, ThreeMonth, SixMonth, Year, AllTime, CurWeek, CurMonth, CurQuarter, CurYear};

		public RadioModeType RadioMode {
			get{return _RadioMode;}
			set{_RadioMode = value;
				OnRepackRadios ();}
		}

		public DateTime DateBegin {
			get{return StartDate.Date;}
			set{StartDate.Date = value;}
		}

		public DateTime DateEnd {
			get{return EndDate.Date;}
			set{EndDate.Date = value;}
		}

		public bool AutoDateSeparation {
			get{return StartDate.AutoSeparation;}
			set{EndDate.AutoSeparation = value;
				StartDate.AutoSeparation = value;}
		}

		public bool IsAllTime {
			get{return StartDate.IsEmpty && EndDate.IsEmpty;}
		}

		public bool ShowToday {
			get{return ShowRadio[Period.Today];}
			set{ShowRadio[Period.Today] = value;
				OnRepackRadios();}
		}

		public bool ShowWeek {
			get{return ShowRadio[Period.Week];}
			set{ShowRadio[Period.Week] = value;
				OnRepackRadios();}
		}

		public bool ShowMonth {
			get{return ShowRadio[Period.Month];}
			set{ShowRadio[Period.Month] = value;
				OnRepackRadios();}
		}

		public bool Show3Month {
			get{return ShowRadio[Period.ThreeMonth];}
			set{ShowRadio[Period.ThreeMonth] = value;
				OnRepackRadios();}
		}

		public bool Show6Month {
			get{return ShowRadio[Period.SixMonth];}
			set{ShowRadio[Period.SixMonth] = value;
				OnRepackRadios();}
		}

		public bool ShowYear {
			get{return ShowRadio[Period.Year];}
			set{ShowRadio[Period.Year] = value;
				OnRepackRadios();}
		}

		public bool ShowAllTime {
			get{return ShowRadio[Period.AllTime];}
			set{ShowRadio[Period.AllTime] = value;
				OnRepackRadios();}
		}

		public bool ShowCurWeek {
			get{return ShowRadio[Period.CurWeek];}
			set{ShowRadio[Period.CurWeek] = value;
				OnRepackRadios();}
		}

		public bool ShowCurMonth {
			get{return ShowRadio[Period.CurMonth];}
			set{ShowRadio[Period.CurMonth] = value;
				OnRepackRadios();}
		}

		public bool ShowCurQuarter {
			get{return ShowRadio[Period.CurQuarter];}
			set{ShowRadio[Period.CurQuarter] = value;
				OnRepackRadios();}
		}

		public bool ShowCurYear {
			get{return ShowRadio[Period.CurYear];}
			set{ShowRadio[Period.CurYear] = value;
				OnRepackRadios();}
		}

		public SelectPeriod ()
		{
			this.Build ();

			//Создаем переключатели
			RadioButtons = new Dictionary<Period, RadioButton>();
			RadioButtons.Add (Period.Today, new RadioButton("Сегодня"));
			RadioButtons[Period.Today].Clicked += OnRadioTodayClicked;
			RadioButton RadioToday = RadioButtons[Period.Today];
			RadioButtons.Add (Period.Week, new RadioButton(RadioToday, "За неделю"));
			RadioButtons[Period.Week].Clicked += OnRadioWeekClicked;
			RadioButtons.Add (Period.Month, new RadioButton(RadioToday, "За месяц"));
			RadioButtons[Period.Month].Clicked += OnRadioMonthClicked;
			RadioButtons.Add (Period.ThreeMonth, new RadioButton(RadioToday, "За 3 месяца"));
			RadioButtons[Period.ThreeMonth].Clicked += OnRadio3monthClicked;
			RadioButtons.Add (Period.SixMonth, new RadioButton(RadioToday, "За полгода"));
			RadioButtons[Period.SixMonth].Clicked += OnRadio6monthClicked;
			RadioButtons.Add (Period.Year, new RadioButton(RadioToday, "За год"));
			RadioButtons[Period.Year].Clicked += OnRadioYearClicked;
			RadioButtons.Add (Period.AllTime, new RadioButton(RadioToday, "Все время"));
			RadioButtons[Period.AllTime].Clicked += OnRadioAllClicked;

			RadioButtons.Add (Period.CurWeek, new RadioButton(RadioToday, "Тек. неделя"));
			RadioButtons[Period.CurWeek].Clicked += OnRadioCurWeekClicked;
			RadioButtons.Add (Period.CurMonth, new RadioButton(RadioToday, "Тек. месяц"));
			RadioButtons[Period.CurMonth].Clicked += OnRadioCurMonthClicked;
			RadioButtons.Add (Period.CurQuarter, new RadioButton(RadioToday, "Тек. квартал"));
			RadioButtons[Period.CurQuarter].Clicked += OnRadioCurQuarterClicked;
			RadioButtons.Add (Period.CurYear, new RadioButton(RadioToday, "Тек. год"));
			RadioButtons[Period.CurYear].Clicked += OnRadioCurYearClicked;

			ShowRadio = new Dictionary<Period, bool>();
			ShowRadio.Add (Period.Today, true);
			ShowRadio.Add (Period.Week, true);
			ShowRadio.Add (Period.Month, true);
			ShowRadio.Add (Period.ThreeMonth, false);
			ShowRadio.Add (Period.SixMonth, false);
			ShowRadio.Add (Period.Year, false);
			ShowRadio.Add (Period.CurWeek, false);
			ShowRadio.Add (Period.CurMonth, false);
			ShowRadio.Add (Period.CurQuarter, false);
			ShowRadio.Add (Period.CurYear, false);
			ShowRadio.Add (Period.AllTime, true);

			_RadioMode = RadioModeType.DoubleColumn;
			OnRepackRadios ();
		}

		public Period ActiveRadio
		{
			get {foreach(KeyValuePair<Period, RadioButton> Pair in RadioButtons)
				{
					if (Pair.Value.Active)
						return Pair.Key;
				};
				return Period.None;
			}
			set {if(value != Period.None)
					RadioButtons[value].Click();
			}
		}

		protected void OnRadioTodayClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.Date;
			EndDate.Date = DateTime.Now.Date;
			UsingRadio = true;
		}

		protected void OnRadioWeekClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddDays(-7);
			EndDate.Date = DateTime.Now.Date;
			UsingRadio = true;
		}

		protected void OnRadioMonthClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddMonths(-1);
			EndDate.Date = DateTime.Now.Date;
			UsingRadio = true;
		}
		
		protected void OnRadio3monthClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddMonths(-3);
			EndDate.Date = DateTime.Now.Date;
			UsingRadio = true;
		}

		protected void OnRadio6monthClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddMonths(-6);
			EndDate.Date = DateTime.Now.Date;
			UsingRadio = true;
		}
		
		protected void OnRadioYearClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddYears(-1);
			EndDate.Date = DateTime.Now.Date;
			UsingRadio = true;
		}
		
		protected void OnRadioAllClicked (object sender, EventArgs e)
		{
			StartDate.Date = new DateTime(1, 1, 1);
			EndDate.Date = new DateTime(1, 1, 1);
			UsingRadio = true;
		}

		protected void OnRadioCurWeekClicked (object sender, EventArgs e)
		{
			int Day;
			if(DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
				Day = 7;
			else 
				Day = (int)DateTime.Now.DayOfWeek;
			StartDate.Date = DateTime.Now.AddDays(1-Day);
			EndDate.Date = DateTime.Now.AddDays(7-Day);
			UsingRadio = true;
		}

		protected void OnRadioCurMonthClicked (object sender, EventArgs e)
		{
			int Year = DateTime.Today.Year;
			int Month = DateTime.Today.Month;
			System.Globalization.Calendar cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
			int LastDay = cal.GetDaysInMonth (Year, Month);
			StartDate.Date = new DateTime(Year, Month, 1);
			EndDate.Date = new DateTime(Year, Month, LastDay);
			UsingRadio = true;
		}

		protected void OnRadioCurQuarterClicked (object sender, EventArgs e)
		{
			int quarterNumber = (DateTime.Today.Month-1)/3+1;
			StartDate.Date = new DateTime(DateTime.Today.Year, (quarterNumber-1)*3+1,1);
			EndDate.Date = StartDate.Date.AddMonths(3).AddDays(-1);
			UsingRadio = true;
		}

		protected void OnRadioCurYearClicked (object sender, EventArgs e)
		{
			StartDate.Date = new DateTime(DateTime.Today.Year, 1, 1);
			EndDate.Date = new DateTime(DateTime.Today.Year, 12, 31);
			UsingRadio = true;
		}

		protected void OnRepackRadios()
		{
			uint Count = 0;
			foreach(KeyValuePair<Period, bool> pair in ShowRadio)
			{
				if(pair.Value)
					Count++;
			}
			uint MaxRow, MaxCol;
			if(_RadioMode == RadioModeType.DoubleColumn)
			{
				MaxCol = 2;
				MaxRow = (Count/2) + (Count%2);
			}
			else
			{
				MaxCol = 1;
				MaxRow = Count;
			}
			foreach(Widget item in RadiosTable.AllChildren)
			{
				RadiosTable.Remove (item);
			}

			RadiosTable.NRows = MaxRow;
			RadiosTable.NColumns = MaxCol;
			uint CurRow = 0;
			uint CurCol = 0;
			foreach(KeyValuePair<Period, bool> pair in ShowRadio)
			{
				if(pair.Value)
				{
					RadiosTable.Attach (RadioButtons[pair.Key], CurCol, CurCol+1, CurRow, CurRow+1);
					CurRow++;
					if(_RadioMode == RadioModeType.DoubleColumn && MaxRow <= CurRow)
					{
						CurRow = 0;
						CurCol++;
					}
				}
			}
			RadiosTable.ShowAll();
		}

		protected void OnStartDateDateChanged (object sender, EventArgs e)
		{
			if(DatesChanged != null)
				DatesChanged(this, EventArgs.Empty);
		}

		protected void OnEndDateDateChanged (object sender, EventArgs e)
		{
			if(DatesChanged != null)
				DatesChanged(this, EventArgs.Empty);
		}
	}
}

