using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;
using Gtk;

namespace QS.Widgets.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class PhoneEntry : Gtk.Entry
	{
		public BindingControler<PhoneEntry> Binding { get; private set; }

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		private PhoneFormat phoneFormat;
		public PhoneFormat PhoneFormat
		{
			get => phoneFormat;
			set
			{
				phoneFormat = value;
				if (PhoneFormat == PhoneFormat.RussiaOnlyHyphenated)
				{
					maxStringLength = 16;
					separator = '-';
				}
			}
		}

		#region Параметры работы
		private int maxStringLength;
		private char separator;

		#endregion

		public PhoneEntry() 
		{
			Binding = new BindingControler<PhoneEntry>(this, new Expression<Func<PhoneEntry, object>>[] {
				(w => w.Text)
			});

			PhoneFormat = PhoneFormat.RussiaOnlyHyphenated;

			this.TextInserted += PhoneTextInserted;
			this.TextDeleted += PhoneTextDeleted;
		}

		protected void PhoneTextInserted (object o, Gtk.TextInsertedArgs args)
		{
			var curPos = args.Position;
			var formated = FormatString (Text, ref curPos);
			var OldTextLegth = Text.Length;
			if(Text != formated)
			{
				Text = formated;
				args.Position = curPos;
			}
		}

		protected void PhoneTextDeleted (object o, Gtk.TextDeletedArgs args)
		{
			if (args.EndPos == -1)
				return;

			int curPos = args.StartPos;
			var formated = FormatString(Text, ref curPos);

			if (Text != formated)
			{
				Text = formated;
				Position = curPos;
			}
		}

		internal string FormatString(string phone, ref int cursorPos)
		{
			string Result = "+7";
			int removeFromStart = 0;
			if(phone.StartsWith("8"))
			{
				removeFromStart = 1;
			}
			else if (phone.StartsWith("+7"))
			{
				removeFromStart = 2;
			}

			string digitsOnly = Regex.Replace(phone.Substring(removeFromStart), "[^0-9]", "");
			if (digitsOnly.Length == 0 && removeFromStart == 0)
				return String.Empty;

			cursorPos = Regex.Replace(phone.Substring(removeFromStart, cursorPos - removeFromStart), "[^0-9]", "").Length + Result.Length;

			Result += digitsOnly;

			foreach(var position in hyphenPositions)
			{
				if (position + 1 > Result.Length)
					break;

				if (position < cursorPos)
					cursorPos++;

				Result = Result.Insert(position, separator.ToString());
			}

			if (Result.Length > maxStringLength)
				return Result.Substring(0, maxStringLength);
			else
				return Result;
		}

		protected override void OnChanged()
		{
			Binding.FireChange(w => w.Text);
			base.OnChanged();
		}

		private int[] hyphenPositions = new int[] { 2, 6, 10, 13 };
	}

	public enum PhoneFormat
	{
		[Display(Name = "+7-XXX-XXX-XX-XX")]
		RussiaOnlyHyphenated
	}
}

