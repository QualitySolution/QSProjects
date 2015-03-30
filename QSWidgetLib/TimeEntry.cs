using System;
using Gtk;
using System.ComponentModel;

namespace QSWidgetLib
{
	[ToolboxItem (true)]
	public class TimeEntry : Gtk.Entry
	{
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
				DateTime.TryParse (Text, out result);
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
				TimeSpan.TryParse (Text, out result);
				return result;
			}
			set {
				if (ShowSeconds)
					Text = value.ToString ("g");
				else
					Text = value.ToString ("hh\\:mm");
			}
		}

		protected override void OnChanged ()
		{
			base.OnChanged ();
			TimeSpan outTime;
			if (TimeSpan.TryParse (Text, out outTime))
				ModifyText (StateType.Normal);
			else
				ModifyText (StateType.Normal, new Gdk.Color (255, 0, 0)); 
		}
	}
}

