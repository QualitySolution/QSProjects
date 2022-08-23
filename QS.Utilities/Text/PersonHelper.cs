using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QS.Utilities.Text
{
	public static class PersonHelper
	{
		public static string PersonNameWithInitials(string lastname, string name, string patronymicName)
		{
			string result = String.Empty;
			if(!String.IsNullOrWhiteSpace(lastname))
				result += String.Format("{0} ", lastname);
			if(!String.IsNullOrWhiteSpace(name))
				result += String.Format("{0}.", name[0]);
			if(!String.IsNullOrWhiteSpace(patronymicName))
				result += String.Format("{0}.", patronymicName[0]);
			return result;
		}

		public static string PersonNameWithInitials(string fioInOneString)
		{
			var parts = fioInOneString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			return PersonNameWithInitials(
				parts.Length > 0 ? parts[0] : null,
				parts.Length > 1 ? parts[1] : null,
				parts.Length > 2 ? parts[2] : null
			);
		}

		public static string PersonFullName(string surname, string name, string patronymicName)
		{
			var parts = new List<string>();

			if(!String.IsNullOrWhiteSpace(surname))
				parts.Add(surname);
			if(!String.IsNullOrWhiteSpace(name))
				parts.Add(name);
			if(!String.IsNullOrWhiteSpace(patronymicName))
				parts.Add(patronymicName);
			return String.Join(" ", parts);
		}

		/// <summary>
		/// Метод разбирает полное имя(ФИО) на части.
		/// Текущая реализация при отсутствии каких-то частей учитывает только порядок следования слов.
		/// </summary>
		/// <param name="fullname">Входящая строка ФИО</param>
		/// <param name="surname">Фамилия</param>
		/// <param name="name">Имя</param>
		/// <param name="patronymicName">Отчество</param>
		public static void SplitFullName(this string fullname, out string surname, out string name, out string patronymicName)
		{
			var parts = fullname.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			surname = parts.Length > 0 ? parts[0] : null;
			name = parts.Length > 1 ? parts[1] : null;

            if(parts.Length > 3)
            {
				patronymicName = $"{parts[2]} {parts[3]}";
			}
			else if(parts.Length > 2)
            {
				patronymicName = parts[2];
			}
			else
            {
				patronymicName = null;
			}
		}
		
		/// <summary>
		/// Метод разбирает фамилию с инициалами на части.
		/// </summary>
		/// <param name="splitNameWithInitials">Входящая строка ФИО</param>
		/// <param name="surname">Фамилия</param>
		/// <param name="name">Первая буква имени</param>
		/// <param name="patronymicName">Первая буква отчества</param>
		public static void SplitNameWithInitials(this string nameWithInitials, out string surname, out string name, out string patronymicName) {
			var result = Regex.Match(nameWithInitials, @"([^ ]+) +(.)\.? *(.)\.?");
			surname = (result.Groups.Count > 1) ? result.Groups[1].Value : null;
			name = (result.Groups.Count > 2) ? result.Groups[2].Value : null;
			patronymicName = (result.Groups.Count > 3) ? result.Groups[3].Value : null;
		}
	}
}
