using System;
using System.Collections.Generic;
using Gtk;

namespace QSWidgetLib
{
	public enum RadioModeType{SingleColumn, DoubleColumn};

	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelectPeriod : Gtk.Bin
	{
		private Dictionary<string, bool> ShowRadio;
		private Dictionary<string, RadioButton> RadioButtons;
		private RadioModeType _RadioMode;
		public event EventHandler DatesChanged;

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

		public bool IsAllTime {
			get{return StartDate.IsEmpty && EndDate.IsEmpty;}
		}

		public bool ShowToday {
			get{return ShowRadio["Today"];}
			set{ShowRadio["Today"] = value;
				OnRepackRadios();}
		}

		public bool ShowWeek {
			get{return ShowRadio["Week"];}
			set{ShowRadio["Week"] = value;
				OnRepackRadios();}
		}

		public bool ShowMonth {
			get{return ShowRadio["Month"];}
			set{ShowRadio["Month"] = value;
				OnRepackRadios();}
		}

		public bool Show3Month {
			get{return ShowRadio["3Month"];}
			set{ShowRadio["3Month"] = value;
				OnRepackRadios();}
		}

		public bool Show6Month {
			get{return ShowRadio["6Month"];}
			set{ShowRadio["6Month"] = value;
				OnRepackRadios();}
		}

		public bool ShowYear {
			get{return ShowRadio["Year"];}
			set{ShowRadio["Year"] = value;
				OnRepackRadios();}
		}

		public bool ShowAllTime {
			get{return ShowRadio["AllTime"];}
			set{ShowRadio["AllTime"] = value;
				OnRepackRadios();}
		}

		public bool ShowCurWeek {
			get{return ShowRadio["CurWeek"];}
			set{ShowRadio["CurWeek"] = value;
				OnRepackRadios();}
		}

		public bool ShowCurMonth {
			get{return ShowRadio["CurMonth"];}
			set{ShowRadio["CurMonth"] = value;
				OnRepackRadios();}
		}

		public bool ShowCurQuarter {
			get{return ShowRadio["CurQuarter"];}
			set{ShowRadio["CurQuarter"] = value;
				OnRepackRadios();}
		}

		public bool ShowCurYear {
			get{return ShowRadio["CurYear"];}
			set{ShowRadio["CurYear"] = value;
				OnRepackRadios();}
		}

		public SelectPeriod ()
		{
			this.Build ();

			//Создаем переключатели
			RadioButtons = new Dictionary<string, RadioButton>();
			RadioButtons.Add ("Today", new RadioButton("Сегодня"));
			RadioButtons["Today"].Clicked += OnRadioTodayClicked;
			RadioButton RadioToday = RadioButtons["Today"];
			RadioButtons.Add ("Week", new RadioButton(RadioToday, "За неделю"));
			RadioButtons["Week"].Clicked += OnRadioWeekClicked;
			RadioButtons.Add ("Month", new RadioButton(RadioToday, "За месяц"));
			RadioButtons["Month"].Clicked += OnRadioMonthClicked;
			RadioButtons.Add ("3Month", new RadioButton(RadioToday, "За 3 месяца"));
			RadioButtons["3Month"].Clicked += OnRadio3monthClicked;
			RadioButtons.Add ("6Month", new RadioButton(RadioToday, "За полгода"));
			RadioButtons["6Month"].Clicked += OnRadio6monthClicked;
			RadioButtons.Add ("Year", new RadioButton(RadioToday, "За год"));
			RadioButtons["Year"].Clicked += OnRadioYearClicked;
			RadioButtons.Add ("AllTime", new RadioButton(RadioToday, "Все время"));
			RadioButtons["AllTime"].Clicked += OnRadioAllClicked;

			RadioButtons.Add ("CurWeek", new RadioButton(RadioToday, "Тек. неделя"));
			RadioButtons["CurWeek"].Clicked += OnRadioCurWeekClicked;
			RadioButtons.Add ("CurMonth", new RadioButton(RadioToday, "Тек. месяц"));
			RadioButtons["CurMonth"].Clicked += OnRadioCurMonthClicked;
			RadioButtons.Add ("CurQuarter", new RadioButton(RadioToday, "Тек. квартал"));
			RadioButtons["CurQuarter"].Clicked += OnRadioCurQuarterClicked;
			RadioButtons.Add ("CurYear", new RadioButton(RadioToday, "Тек. год"));
			RadioButtons["CurYear"].Clicked += OnRadioYearClicked;

			ShowRadio = new Dictionary<string, bool>();
			ShowRadio.Add ("Today", true);
			ShowRadio.Add ("Week", true);
			ShowRadio.Add ("Month", true);
			ShowRadio.Add ("3Month", false);
			ShowRadio.Add ("6Month", false);
			ShowRadio.Add ("Year", false);
			ShowRadio.Add ("CurWeek", false);
			ShowRadio.Add ("CurMonth", false);
			ShowRadio.Add ("CurQuarter", false);
			ShowRadio.Add ("CurYear", false);
			ShowRadio.Add ("AllTime", true);

			_RadioMode = RadioModeType.DoubleColumn;
			OnRepackRadios ();
		}

		protected void OnRadioTodayClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.Date;
			EndDate.Date = DateTime.Now.Date;
		}

		protected void OnRadioWeekClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddDays(-7);
			EndDate.Date = DateTime.Now.Date;
		}

		protected void OnRadioMonthClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddMonths(-1);
			EndDate.Date = DateTime.Now.Date;
		}
		
		protected void OnRadio3monthClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddMonths(-3);
			EndDate.Date = DateTime.Now.Date;
		}

		protected void OnRadio6monthClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddMonths(-6);
			EndDate.Date = DateTime.Now.Date;
		}
		
		protected void OnRadioYearClicked (object sender, EventArgs e)
		{
			StartDate.Date = DateTime.Now.AddYears(-1);
			EndDate.Date = DateTime.Now.Date;
		}
		
		protected void OnRadioAllClicked (object sender, EventArgs e)
		{
			StartDate.Date = new DateTime(1, 1, 1);
			EndDate.Date = new DateTime(1, 1, 1);
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
		}

		protected void OnRadioCurMonthClicked (object sender, EventArgs e)
		{
			int Year = DateTime.Today.Year;
			int Month = DateTime.Today.Month;
			System.Globalization.Calendar cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
			int LastDay = cal.GetDaysInMonth (Year, Month);
			StartDate.Date = new DateTime(Year, Month, 1);
			EndDate.Date = new DateTime(Year, Month, LastDay);
		}

		protected void OnRadioCurQuarterClicked (object sender, EventArgs e)
		{
			int quarterNumber = (DateTime.Today.Month-1)/3+1;
			StartDate.Date = new DateTime(DateTime.Today.Year, (quarterNumber-1)*3+1,1);
			EndDate.Date = StartDate.Date.AddMonths(3).AddDays(-1);
		}

		protected void OnRadioCurYearClicked (object sender, EventArgs e)
		{
			StartDate.Date = new DateTime(DateTime.Today.Year, 1, 1);
			EndDate.Date = new DateTime(DateTime.Today.Year, 12, 31);;
		}

		protected void OnRepackRadios()
		{
			uint Count = 0;
			foreach(KeyValuePair<string, bool> pair in ShowRadio)
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
			foreach(KeyValuePair<string, bool> pair in ShowRadio)
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

