using System;
using System.Text.RegularExpressions;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ValidatedEntry : Gtk.Entry
	{
		public enum Validation {
			none,
			numeric,
			email,
			single_word,
			phone
		};

		private Validation validationType;

		public Validation ValidationType {
			get {
				return validationType;
			}
			set { 
				validationType = value;
				switch (validationType) {
				case Validation.numeric:
					this.TextInserted += NumericValidate;
					break;
				case Validation.phone:
					//this.TextInserted += PhoneValidate;
					//this.TextDeleted += DeleteValidate;
					break;
				case Validation.single_word:
					break;
				case Validation.email:
					this.EditingDone += EmailValidate;
					break;
				default:
					validationType = Validation.numeric;
					this.TextInserted += NumericValidate;
					break;
				}
			}
		}

		public ValidatedEntry () 
		{
			ValidationType = Validation.none;
		}

		protected void NumericValidate(object sender, Gtk.TextInsertedArgs Args)
		{
			String Text = (sender as Gtk.Entry).Text;
			Text = Regex.Replace (Text, "[^0-9]", "");
			(sender as Gtk.Entry).Text = Text;
		}

		protected void EmailValidate(object sender, System.EventArgs Args)
		{
			String Text = (sender as Gtk.Entry).Text;
			Regex regex = new Regex (@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@" +
							@"[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]\.[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]$");
			if (!regex.IsMatch(Text))
				(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal, new Gdk.Color(120,0,0));
			else
				(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal, new Gdk.Color(0,0,0));
		}
	}
}

