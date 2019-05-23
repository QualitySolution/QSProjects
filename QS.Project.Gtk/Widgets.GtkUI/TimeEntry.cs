using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem (true)]
	[Category("QS.Project")]
	public class TimeEntry : Gtk.Entry
	{
		static readonly char[] timeSeparators={'-', ':', '.','/','\\',','};

		public BindingControler<TimeEntry> Binding { get; private set; }

		bool showSeconds;

		public bool ShowSeconds {
			get {
				return showSeconds;
			}
			set {
				MaxLength = value ? 8 : 5;
				showSeconds = value;
			}
		}

		byte autocompleteStep;

		public byte AutocompleteStep {
			get {
				return autocompleteStep;
			}
			set {
				autocompleteStep = value;
				if (value != 0) {
					var autocomplete = new ListStore (typeof(string));
					for (DateTime time = new DateTime (); time < new DateTime ().AddDays (1); time = time.AddMinutes (value))
						autocomplete.AppendValues (showSeconds ? time.ToLongTimeString () : time.ToShortTimeString ());
					Completion = new EntryCompletion ();
					Completion.Model = autocomplete;
					Completion.TextColumn = 0;
				}
			}
		}

		public DateTime DateTime {
			get {
				DateTime result;
				TryParseDateTime (Text, out result);
				return result;
			}
			set {
				if (ShowSeconds)
					Text = value.ToLongTimeString ();
				else
					Text = value.ToShortTimeString ();
			}
		}

		public TimeSpan Time {
			get {
				TimeSpan result;
				TryParseTimeSpan (Text, out result);
				return result;
			}
			set {
				if (ShowSeconds)
					Text = value.ToString ("g");
				else
					Text = value.ToString ("hh\\:mm");
			}
		}

		protected bool TryParseTimeSpan(string text, out TimeSpan result){
			CultureInfo culture = CultureInfo.InvariantCulture;
			string formattedText;
			result = new TimeSpan ();
			return TryFormat(text, out formattedText) 
				&& TimeSpan.TryParseExact (formattedText, "hh\\:mm", culture, out result);
		}

		protected bool TryParseDateTime(string text, out DateTime result){
			CultureInfo culture = CultureInfo.InvariantCulture;
			string formattedText;
			result = new DateTime();
			return TryFormat(text, out formattedText) 
				&& DateTime.TryParseExact (formattedText, "hh\\:mm", culture, DateTimeStyles.None, out result);
		}

		protected bool TryFormat(string text,out string result){
			var split = text.Split (timeSeparators);
			if (split.Length > 2) {
				result = "";
				return false;
			} else if (split.Length == 1) {
				string padded = (text.Length < 3) 
					? text.PadRight (text.Length + 2, '0')
					: text;	
				split = new string[]{
					padded.Substring(0,padded.Length-2),
					padded.Substring(padded.Length-2,2)
				};
			}
			string hours = split[0];
			string minutes = split [1];
			hours = hours.PadLeft (2, '0');
			minutes = minutes.PadLeft (2, '0');
			result = hours + ":" + minutes;
			return true;
		}
			
		protected override void OnChanged ()
		{
			Binding.FireChange(new Expression<Func<TimeEntry, object>>[] {
				(w => w.DateTime),
				(w => w.Time),
			});

			base.OnChanged ();
			TimeSpan outTime;
			if (TryParseTimeSpan(Text,out outTime))
				ModifyText (StateType.Normal);
			else
				ModifyText (StateType.Normal, new Gdk.Color (255, 0, 0)); 
		}

		protected override bool OnFocusOutEvent (Gdk.EventFocus evnt)
		{
			var result = base.OnFocusOutEvent (evnt);
			TimeSpan timeSpan;
			if(TryParseTimeSpan(Text,out timeSpan)){
				Text = timeSpan.ToString();
			}
			return result;
		}

		public TimeEntry()
		{
			Binding = new BindingControler<TimeEntry>(this, new Expression<Func<TimeEntry, object>>[] {
				(w => w.DateTime),
				(w => w.Time),
			});

			//Что бы установить ограничения на максимальное количество символов, по умолчанию.
			ShowSeconds = false; 
		}
	}
}

