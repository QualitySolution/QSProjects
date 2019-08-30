using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace QS.Utilities.Numeric
{
	public class PhoneFormatter
	{
		public PhoneFormat Format;

		#region Настройки форматов
		private int MaxStringLength => Format == PhoneFormat.RussiaOnlyHyphenated ? 16 : 0;
		private string Separator => Format == PhoneFormat.RussiaOnlyHyphenated ? "-" : "";
		private int[] SeparatorPositions => Format == PhoneFormat.RussiaOnlyHyphenated ? hyphenPositions : new int[] { };

		private int[] hyphenPositions = new int[] { 2, 6, 10, 13 };
		#endregion

		public string FormatString(string phone)
		{
			int fake = 0;
			return FormatString(phone, ref fake);
		}

		public string FormatString(string phone, ref int cursorPos)
		{
			string Result = "+7";
			string startTrimed = Regex.Replace(phone, "^[^0-9\\+]+", "");
			int removeFromStart = phone.Length - startTrimed.Length;
			if (startTrimed.StartsWith("8")) {
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

			foreach (var position in SeparatorPositions) {
				if (position + 1 > Result.Length)
					break;

				if (position < cursorPos)
					cursorPos++;

				Result = Result.Insert(position, Separator);
			}

			if (Result.Length > MaxStringLength)
				return Result.Substring(0, MaxStringLength);
			else
				return Result;
		}

	}

	public enum PhoneFormat
	{
		[Display(Name = "+7-XXX-XXX-XX-XX")]
		RussiaOnlyHyphenated
	}
}
