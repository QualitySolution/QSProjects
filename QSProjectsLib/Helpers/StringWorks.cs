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
			{ "ОПК", "Огороднический потребительский кооператив" },
			{ "ГБПОУ", "Государственное бюджетное профессиональное образовательное учреждение" },
			{ "МБУ", "Муниципальное бюджетное учреждение" },
			{ "ФБУН", "Федеральное бюджетное учреждение науки" },
			{ "ГБУ ЛО", "Государственное бюджетное учреждение ленинградской области" },
			{ "ГБУ", "Государственное бюджетное учреждение" },
			{ "ПАО", "Публичное акционерное общество" },
			{ "ФКП", "Федеральное казенное предприятие" },
			{ "АМО", "Администрация муниципального образования" },
			{ "МКУ", "Муниципальное казенное учреждение" },
			{ "МС", "Муниципальный совет" },
			{ "МА", "Местная администрация" },
			{ "СПБГБУ", "Санкт-Петербургское государственное бюджетное учреждение" },
			{ "СПБГАУК", "Санкт-Петербургское государственное автономное учреждение культуры" },
			{ "СПБГБУК", "Санкт-Петербургское государственное бюджетное учреждение культуры" },
			{ "ФГБУН", "Федеральное государственное бюджетное учреждение науки" },
			{ "МКУК", "Муниципальное казенное учреждение культуры" }
		};

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

		[Obsolete("Используйте аналогичный функционал из QS.Utilities.Text.VersionHelper.")]
		public static string VersionToShortString (Version version)
		{
			return version.ToString (version.Revision <= 0 ? (version.Build <= 0 ? 2 : 3) : 4);
		}

		[Obsolete("Используйте аналогичный функционал из QS.Utilities.Text.VersionHelper.")]
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

