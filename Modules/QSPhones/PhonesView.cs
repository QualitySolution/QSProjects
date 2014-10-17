using System;
using System.Text.RegularExpressions;

namespace QSPhones
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PhonesView : Gtk.Bin
	{
		public PhonesView ()
		{
			this.Build ();
			entryPhone.IsEditable = true;
			entryAdditional.IsEditable = true;
			entryPhone.MaxLength = 19;
		}

		protected void OnEntryPhoneTextInserted (object o, Gtk.TextInsertedArgs args)
		{
			FormatString ();
			switch (args.Position) {
			case 1:
				args.Position += 1;
				break;
			case 5:
				args.Position += 2;
				break;
			case 10:
				args.Position += 3;
				break;
			case 15:
				args.Position += 3;
				break;
			}
		}

		protected void OnEntryPhoneTextDeleted (object o, Gtk.TextDeletedArgs args)
		{
			FormatString ();
			if (args.StartPos > entryPhone.Text.Length)
				entryPhone.Position = entryPhone.Text.Length;
			else
				entryPhone.Position = args.StartPos;
			if (args.StartPos == 16 && args.EndPos == 17) {			//Backspace
				entryPhone.Text = entryPhone.Text.Remove (13, 1);
				entryPhone.Position = 13;
			} else if (args.StartPos == 11 && args.EndPos == 12) {
				entryPhone.Text = entryPhone.Text.Remove (8, 1);
				entryPhone.Position = 8;
			} else if (args.StartPos == 5 && args.EndPos == 6) {
				entryPhone.Text = entryPhone.Text.Remove (3, 1);
				entryPhone.Position = 3;
			} else if (args.StartPos == 14 && args.EndPos == 15) { 	//Delete
				entryPhone.Text = entryPhone.Text.Remove (17, 1);
				entryPhone.Position = 17;
			} else if (args.StartPos == 9 && args.EndPos == 10) {
				entryPhone.Text = entryPhone.Text.Remove (12, 1);
				entryPhone.Position = 12;
			} else if (args.StartPos == 4 && args.EndPos == 5) {
				entryPhone.Text = entryPhone.Text.Remove (6, 1);
				entryPhone.Position = 6;
			}
		}

		private void FormatString()
		{
			string Number = entryPhone.Text;
			Number = Regex.Replace (Number, "[^0-9]", "");
			if (Number != String.Empty) {
				if (Number.Length > 0)
					Number = Number.Insert (0, "(");
				if (Number.Length > 4)
					Number = Number.Insert (4, ") ");
				if (Number.Length > 9)
					Number = Number.Insert (9, " - ");
				if (Number.Length > 14)
					Number = Number.Insert (14, " - ");
				entryPhone.Text = Number;
			}
		}
	}
}

