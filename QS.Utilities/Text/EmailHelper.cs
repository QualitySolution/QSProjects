using System;
using System.Text.RegularExpressions;

namespace QS.Utilities.Text {
	public static class EmailHelper {
		public static bool Validate(string email, bool emptyIsValid = true)
		{
			if(String.IsNullOrWhiteSpace(email))
				return emptyIsValid;
			return Regex.IsMatch(email, @"^[a-zA-Z0-9]+([\._-]?[a-zA-Z0-9]+)*@[a-zA-Z0-9]+([\.-]?[a-zA-Z0-9]+)*(\.[a-zA-Z]{2,10})+$");
		}
	}
}
