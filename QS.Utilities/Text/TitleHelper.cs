using System;
using System.Globalization;

namespace QS.Utilities.Text
{
	public static class TitleHelper
	{
		public static string StringToPascalCase(this string input)
		{
			if(input == null)
				return "";

			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
			string result = textInfo.ToTitleCase(input.Trim()).Replace(" ", "");

			return result;
		}

		public static string StringToTitleCase(this string input)
		{
			if(String.IsNullOrWhiteSpace(input))
				return "";

			TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

			string result = input.Trim();
			result = textInfo.ToUpper(result[0]) + result.Substring(1);

			return result;
		}
	}
}
