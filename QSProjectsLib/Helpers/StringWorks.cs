using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace QSProjectsLib
{
	public class StringWorks
	{
		enum PowTwo : ulong
		{
			Pow10 = 1024,
			Pow20 = 1048576,
			Pow30 = 1073741824,
			Pow40 = 1099511627776,
			Pow50 = 1125899906842620,
			Pow60 = 1152921504606850000
		}

		public static Dictionary<string, string> Replaces = new Dictionary<string, string> {
			{ "ООО", "Общество с ограниченной ответственностью" },
			{ "ЗАО", "Закрытое акционерное общество" },
			{ "ОАО", "Открытое акционерное общество" },
			{ "ГорПО", "Городское потребительское общество" },
			{ "РайПО", "Районное потребительское общество" },
			{ "СельПО", "Сельское потребительское общество" },
			{ "НКО", "Некоммерческая организация" },
			{ "ИП", "Индивидуальный предприниматель" },
			{ "ОДО", "Общество с дополнительной ответственностью" },
			{ "ПТ", "Полное товарищество" },
			{ "АО", "Акционерное общество" },
			{ "КТ", "Коммандитное товарищество" },
			{ "ХП", "Хозяйственное партнерство" },
			{ "КФХ", "Крестьянское (фермерское) хозяйство" },
			{ "НП", "Некоммерческое партнерство" },
			{ "ПК", "Потребительский кооператив" },
			{ "ТСН", "Товарищество собственников недвижимости" },
			{ "АНО", "Автономная некоммерческая организация" },
			{ "ФГУП", "Федеральное государственное унитарное предприятие" },
			{ "ГУП", "Государственное унитарное предприятие" },
			{ "МУП", "Муниципальное унитарное предприятие" },
			{ "ГП", "Государственное предприятие" },
			{ "СНТ", "Садоводческое некоммерческое товарищество" },
			{ "ДНТ", "Дачное некоммерческое товарищество" },
			{ "ОНТ", "Огородническое некоммерческое товарищество" },
			{ "СНП", "Садоводческое некоммерческое партнерство" },
			{ "ДНП", "Дачное некоммерческое партнерство" },
			{ "ОНП", "Огородническое некоммерческое партнерство" },
			{ "СПК", "Садоводческий потребительский кооператив" },
			{ "ДПК", "Дачный потребительский кооператив" },
			{ "ОПК", "Огороднический потребительский кооператив" }
		};

		public class NaturalStringComparerNonStatic : IComparer<string>
		{
			public int Compare (string x, string y)
			{
				return NaturalStringComparer.Compare (x, y);
			}
		}

		public class NaturalStringComparer
		{
			private static readonly Regex _re = new Regex (@"(?<=\D)(?=\d)|(?<=\d)(?=\D)", RegexOptions.Compiled);

			public static int Compare (string x, string y)
			{
				x = x.ToLower ();
				y = y.ToLower ();
				if (string.Compare (x, 0, y, 0, Math.Min (x.Length, y.Length)) == 0) {
					if (x.Length == y.Length)
						return 0;
					return x.Length < y.Length ? -1 : 1;
				}
				var a = _re.Split (x);
				var b = _re.Split (y);
				int i = 0;
				while (true) {
					int r = PartCompare (a [i], b [i]);
					if (r != 0)
						return r;
					++i;
				}
			}

			private static int PartCompare (string x, string y)
			{
				int a, b;
				if (int.TryParse (x, out a) && int.TryParse (y, out b))
					return a.CompareTo (b);
				return x.CompareTo (y);
			}
		}

		[Obsolete("Используйте аналогичный функционал из QS.Utilities.Text.PersonHelper.")]
		public static string PersonNameWithInitials (string lastname, string name, string patronymicName)
		{
			string result = String.Empty;
			if (!String.IsNullOrWhiteSpace (lastname))
				result += String.Format ("{0} ", lastname);
			if (!String.IsNullOrWhiteSpace (name))
				result += String.Format ("{0}.", name [0]);
			if (!String.IsNullOrWhiteSpace (patronymicName))
				result += String.Format ("{0}.", patronymicName [0]);
			return result;
		}

		[Obsolete("Используйте аналогичный функционал из QS.Utilities.Text.PersonHelper.")]
		public static string PersonNameWithInitials (string fioInOneString)
		{
			var parts = fioInOneString.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
			return PersonNameWithInitials(
				parts.Length > 0 ? parts[0] : null,
				parts.Length > 1 ? parts[1] : null,
				parts.Length > 2 ? parts[2] : null
			);
		}

		[Obsolete("Используйте аналогичный функционал из QS.Utilities.Text.PersonHelper.")]
		public static string PersonFullName (string surname, string name, string patronymicName)
		{
			var parts = new List<string>();

			if (!String.IsNullOrWhiteSpace(surname))
				parts.Add(surname);
			if (!String.IsNullOrWhiteSpace (name))
				parts.Add(name);
			if (!String.IsNullOrWhiteSpace (patronymicName))
				parts.Add(patronymicName);
			return String.Join(" ", parts);
		}

		public static string BytesToIECUnitsString (ulong bytes)
		{
			if (bytes < (ulong)PowTwo.Pow10)
				return String.Format ("{0} Б", bytes);
			else if (bytes < (ulong)PowTwo.Pow20)
				return String.Format ("{0:N1} КиБ", (double)bytes / (ulong)PowTwo.Pow10);
			else if (bytes < (ulong)PowTwo.Pow30)
				return String.Format ("{0:N1} МиБ", (double)bytes / (ulong)PowTwo.Pow20);
			else if (bytes < (ulong)PowTwo.Pow40)
				return String.Format ("{0:N1} ГиБ", (double)bytes / (ulong)PowTwo.Pow30);
			else if (bytes < (ulong)PowTwo.Pow50)
				return String.Format ("{0:N1} ТиБ", (double)bytes / (ulong)PowTwo.Pow40);
			else if (bytes < (ulong)PowTwo.Pow60)
				return String.Format ("{0:N1} ПиБ", (double)bytes / (ulong)PowTwo.Pow50);
			else
				return String.Format ("{0:N1} ЭиБ", (double)bytes / (ulong)PowTwo.Pow60);
		}

		[Obsolete("Используйте аналогичный функционал из QS.Utilities.Text.TitleHelper.")]
		public static string StringToPascalCase (string input)
		{
			if (input == null)
				return "";

			TextInfo textInfo = new CultureInfo ("en-US", false).TextInfo;
			string result = textInfo.ToTitleCase (input.Trim ()).Replace (" ", "");

			return result;
		}

		[Obsolete("Используйте аналогичный функционал из QS.Utilities.Text.TitleHelper.")]
		public static string StringToTitleCase (string input)
		{
			if (String.IsNullOrWhiteSpace (input))
				return "";

			TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

			string result = input.Trim ();
			result = textInfo.ToUpper (result [0]) + result.Substring (1);

			return result;
		}

		public static string VersionToShortString (Version version)
		{
			return version.ToString (version.Revision <= 0 ? (version.Build <= 0 ? 2 : 3) : 4);
		}

		public static string VersionToShortString (string version)
		{
			var ver = Version.Parse (version);
			return ver.ToString (ver.Revision == 0 ? (ver.Build == 0 ? 2 : 3) : 4);
		}

		public static string EllipsizeEnd(string text, int length, bool wholeWord = false) {
			if (text.Length <= length) return text;
			length -= 3;
			int pos = wholeWord ? text.IndexOf(" ", length) : length;
			if (pos == -1)
				pos = length;
			if (pos >= 0)
				return text.Substring(0, pos) + "...";
			return text;
		}
	}
}

