using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace QS.Utilities.Numeric
{
	public class PhoneFormatter
	{
		public readonly PhoneFormat Format;

		#region Настройки форматов
		private int MaxStringLength;
		private string Starttext;
		private SeparatorPosition[] SeparatorPositions;

		public PhoneFormatter(PhoneFormat format)
		{
			Format = format;
			switch (Format) {
				case PhoneFormat.RussiaOnlyHyphenated:
					MaxStringLength = 16;
					Starttext = "+7";
					SeparatorPositions = new SeparatorPosition[] { new SeparatorPosition(2, "-"), new SeparatorPosition(6, "-"), new SeparatorPosition(10, "-"), new SeparatorPosition(13, "-") };
					break;
				case PhoneFormat.BracketWithWhitespaceLastTen:
					MaxStringLength = 19;
					Starttext = "";
					SeparatorPositions = new SeparatorPosition[] { new SeparatorPosition(0, "("), new SeparatorPosition(4, ") "), new SeparatorPosition(9, " - "), new SeparatorPosition(14, " - ")};
					break;
				case PhoneFormat.DigitsTen:
					MaxStringLength = 10;
					Starttext = "";
					SeparatorPositions = new SeparatorPosition[] {new SeparatorPosition(0,"") };
					break;
			}
		}
		#endregion

		public string FormatString(string phone)
		{
			int fake = 0;
			return FormatString(phone, ref fake);
		}

		public string FormatString(string phone, ref int cursorPos)
		{
			string Result = Starttext;
			string startTrimed = Regex.Replace(phone, "^[^0-9\\+]+", "");
			int removeFromStart = phone.Length - startTrimed.Length;
			if (startTrimed.StartsWith("8") || startTrimed.StartsWith("7")) {
				removeFromStart += 1;
			}
			else if (startTrimed.StartsWith("+7")) {
				removeFromStart += 2;
			}

			string digitsOnly = Regex.Replace(phone.Substring(removeFromStart), "[^0-9]", "");
			if (digitsOnly.Length == 0 && removeFromStart == 0)
				return String.Empty;

			var strBeforeCursor = phone.Substring(removeFromStart, cursorPos - removeFromStart < 0 ? 0 : cursorPos - removeFromStart);
			cursorPos = Regex.Replace(strBeforeCursor, "[^0-9]", "").Length + Result.Length;

			Result += digitsOnly;

			foreach (var point in SeparatorPositions) {
				if (point.position + 1 > Result.Length)
					break;

				if (point.position < cursorPos)
					cursorPos++;

				Result = Result.Insert(point.position, point.separator);
			}

			if (Result.Length > MaxStringLength)
				return Result.Substring(0, MaxStringLength);
			else
				return Result;
		}

	}

	class SeparatorPosition {
		public int position;
		public string separator;

		public SeparatorPosition(int position, string separator)
		{
			this.position = position;
			this.separator = separator;
		}
	}

	public enum PhoneFormat
	{
		/// <summary>
		/// +7-XXX-XXX-XX-XX
		/// </summary>
		[Display(Name = "+7-XXX-XXX-XX-XX")]
		RussiaOnlyHyphenated,
		/// <summary>
		/// (XXX) XXX - XX - XX
		/// </summary>
		[Display(Name = "(XXX) XXX - XX - XX")]
		BracketWithWhitespaceLastTen,
		/// <summary>
		/// XXXXXXXXXX
		/// </summary>
		[Display(Name = "XXXXXXXXXX")]
		DigitsTen
	}
}
