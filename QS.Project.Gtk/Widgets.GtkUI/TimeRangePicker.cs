using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class TimeRangePicker : Bin
	{
		public BindingControler<TimeRangePicker> Binding { get; private set; }

		protected TimeSpan timeStart = TimeSpan.MinValue;
		protected TimeSpan? timeEnd;
		public event EventHandler TimePeriodChanged;
		public event EventHandler TimePeriodChangedByUser;
		protected Gtk.Dialog editRange;

		public TimeRangePicker()
		{
			this.Build();
			entTimeRange.MaxLength = entTimeRange.WidthChars = 13;
			Binding = new BindingControler<TimeRangePicker>(
				this,
				new Expression<Func<TimeRangePicker, object>>[] {
					(w => w.TimeStart),
					(w => w.TimeEnd)
				}
			);
			btnSetRange.Clicked += BtnSetRange_Clicked;
			TimePeriodChanged += (sender, e) => UpdateEntryText();
		}

		public TimeSpan TimeStart {
			get => timeStart;
			set {
				if(timeStart != value) {
					timeStart = value;
					OnStartTimeChanged();
				}
			}
		}

		public TimeSpan? TimeEnd {
			get => timeEnd ?? TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1));
			set {
				if(timeEnd != value) {
					timeEnd = value;
					OnEndTimeChanged();
				}
			}
		}

		protected virtual void OnStartTimeChanged()
		{
			Binding.FireChange(
				new Expression<Func<TimeRangePicker, object>>[] {
					(w => w.TimeStart)
				}
			);

			TimePeriodChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnEndTimeChanged()
		{
			Binding.FireChange(
				new Expression<Func<TimeRangePicker, object>>[] {
					(w => w.TimeEnd)
				}
			);

			TimePeriodChanged?.Invoke(this, EventArgs.Empty);
		}

		void UpdateEntryText()
		{
			if(!TimeEnd.HasValue)
				entTimeRange.Text = string.Format("с {0}", TimeStart.ToString("hh\\:mm"));
			entTimeRange.Text = string.Format("{0} - {1}", TimeStart.ToString("hh\\:mm"), TimeEnd.Value.ToString("hh\\:mm"));
		}

		void BtnSetRange_Clicked(object sender, EventArgs e)
		{
			#region Widget creation
			Window parentWin = (Window)Toplevel;
			var selectTimeRange = new Gtk.Dialog("Укажите период", parentWin, DialogFlags.DestroyWithParent) {
				Modal = true
			};
			selectTimeRange.AddButton("Отмена", ResponseType.Cancel);
			selectTimeRange.AddButton("Ok", ResponseType.Ok);

			var timeStartEntry = new TimeEntry {
				Time = TimeStart,
				AutocompleteStep = 5
			};
			var timeStartScale = new HScale(-1439.9999, 0, 5) {
				DrawValue = false,
				Value = -TimeStart.TotalMinutes,
				Inverted = true
			};
			timeStartScale.Adjustment.PageIncrement = -60;
			timeStartScale.ValueChanged += (o, args) => {
				if(!timeStartEntry.HasFocus)
					timeStartEntry.Time = TimeSpan.FromMinutes(-timeStartScale.Value);
			};
			timeStartEntry.Changed += (s, ea) => timeStartScale.Value = -timeStartEntry.Time.TotalMinutes;
			var timeStartCtrlBox = new VBox {
				timeStartEntry,
				timeStartScale
			};

			var timeEndEntry = new TimeEntry {
				Time = TimeEnd ?? TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1)),
				AutocompleteStep = 5
			};
			var timeEndScale = new HScale(0, 1439.9999, 5) {
				DrawValue = false,
				Value = (TimeEnd ?? TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1))).TotalMinutes
			};
			timeEndScale.Adjustment.PageIncrement = 60;
			timeEndScale.ValueChanged += (o, args) => {
				if(!timeEndEntry.HasFocus)
					timeEndEntry.Time = TimeSpan.FromMinutes(timeEndScale.Value);
			};
			timeEndEntry.Changed += (s, ea) => timeEndScale.Value = timeEndEntry.Time.TotalMinutes;
			var timeEndCtrlBox = new VBox {
				timeEndEntry,
				timeEndScale
			};

			timeStartEntry.Changed += TimeValidator;
			timeEndEntry.Changed += TimeValidator;
			timeStartScale.ValueChanged += TimeValidator;
			timeEndScale.ValueChanged += TimeValidator;

			void TimeValidator(object objectSender, EventArgs eventArgs)
			{
				if(timeStartEntry.Time.TotalMinutes > timeEndEntry.Time.TotalMinutes) {
					timeStartEntry.ModifyBase(StateType.Normal, new Gdk.Color(255, 0, 0));
					timeEndEntry.ModifyBase(StateType.Normal, new Gdk.Color(255, 0, 0));
				} else {
					timeStartEntry.ModifyBase(StateType.Normal, new Gdk.Color(255, 255, 255));
					timeEndEntry.ModifyBase(StateType.Normal, new Gdk.Color(255, 255, 255));
				}
			}

			HBox timeRangeBox = new HBox {
				timeStartCtrlBox,
				timeEndCtrlBox
			};

			selectTimeRange.VBox.Add(timeRangeBox);
			selectTimeRange.ShowAll();
			#endregion

			int response = selectTimeRange.Run();
			if(response == (int)ResponseType.Ok && timeStartEntry.Time.TotalMinutes <= timeEndEntry.Time.TotalMinutes) {
				timeStart = timeStartEntry.Time;
				timeEnd = timeEndEntry.Time;
				OnStartTimeChanged();
				OnEndTimeChanged();
				TimePeriodChangedByUser?.Invoke(this, EventArgs.Empty);
			}

			#region Destroy
			selectTimeRange.Destroy();
			#endregion
		}

	}
}