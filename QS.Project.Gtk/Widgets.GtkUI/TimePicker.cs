using Gamma.Binding.Core;
using Gtk;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace QS.Widgets.GtkUI {
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class TimePicker : Bin {
		public BindingControler<TimePicker> Binding { get; private set; }

		protected TimeSpan? timeOrNull;
		public event EventHandler TimeChanged;
		public event EventHandler TimeChangedByUser;
		public Func<TimeSpan> GetDefaultTime;

		public TimePicker() {
			Build();
			entryTime.MaxLength = entryTime.WidthChars = 5;
			Binding = new BindingControler<TimePicker>(
				this,
				new Expression<Func<TimePicker, object>>[] {
					(w => w.TimeOrNull)
				}
			);
			buttonSetTime.Clicked += OnButtonSetTimeClicked;
			buttonClearTime.Clicked += OnButtonClearTimeClicked;
			TimeChanged += OnTimeChanged;
		}

		private void OnTimeChanged(object sender, EventArgs e) {
			entryTime.Text = TimeOrNull?.ToString("hh\\:mm") ?? "";
		}
		private void OnButtonClearTimeClicked(object sender, EventArgs e) {
			timeOrNull = null;
			OnTimeChanged();
		}

		public TimeSpan? TimeOrNull {
			get => timeOrNull;
			set {
					timeOrNull = value;
					OnTimeChanged();				
			}
		}

		protected virtual void OnTimeChanged() {
			Binding.FireChange(
				new Expression<Func<TimePicker, object>>[] {
					(w => w.TimeOrNull)
				}
			);

			TimeChanged?.Invoke(this, EventArgs.Empty);
		}

		void OnButtonSetTimeClicked(object sender, EventArgs e) {
			#region Widget creation

			var isPreviousEmpty = false;

			if(!TimeOrNull.HasValue) {
				isPreviousEmpty = true;
				if(GetDefaultTime != null) {
					timeOrNull = GetDefaultTime();
				}
				else {
					timeOrNull = new TimeSpan(0, 0, 0);
				}
			}

			Window parentWin = (Window)Toplevel;
			var selectTime = new Gtk.Dialog("Укажите время", parentWin, DialogFlags.DestroyWithParent) {
				Modal = true
			};
			selectTime.AddButton("Отмена", ResponseType.Cancel);
			selectTime.AddButton("Ok", ResponseType.Ok);

			var timeEntry = new TimeEntry {
				AutocompleteStep = 5
			};

			var timeScale = new HScale(-1439.9999, 0, 5) {
				DrawValue = false,
				Inverted = true
			};

			if(TimeOrNull.HasValue) {
				timeEntry.Time = TimeOrNull.Value;
				timeScale.Value = -TimeOrNull.Value.TotalMinutes;
			}

			timeScale.Adjustment.PageIncrement = -60;
			timeScale.ValueChanged += (s, a) => {
				if(!timeEntry.HasFocus)
					timeEntry.Time = TimeSpan.FromMinutes(-timeScale.Value);
			};

			timeEntry.Changed += (s, a) => timeScale.Value = -timeEntry.Time.TotalMinutes;
			var timeStartCtrlBox = new VBox {
				timeEntry,
				timeScale
			};

			HBox timeBox = new HBox {
				timeStartCtrlBox
			};

			selectTime.VBox.Add(timeBox);
			selectTime.ShowAll();
			#endregion

			int response = selectTime.Run();
			if(response == (int)ResponseType.Ok) {
				timeOrNull = timeEntry.Time;
				OnTimeChanged();
				TimeChangedByUser?.Invoke(this, EventArgs.Empty);
			}
			else {
				if(isPreviousEmpty) {
					timeOrNull = null;
				}
			}

			#region Destroy
			selectTime.Destroy();
			#endregion
		}
	}
}
