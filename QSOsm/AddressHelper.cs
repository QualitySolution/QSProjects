using System;

namespace QSOsm
{
	public static class AddressHelper
	{
		public static string ShortenCity(string city)
		{
			if (city == "Санкт-Петербург")
				return "Спб";
			return city;
		}

		public static string ShortenStreet(string street)
		{
			//Сокращаем статусную часть.
			if (street.Contains("улица"))
				street.Replace("улица", "ул.");
			else if (street.Contains("проспект"))
				street.Replace("проспект", "пр.");
			else if (street.Contains("бульвар"))
				street.Replace("бульвар", "бульв.");
			else if (street.Contains("дорога"))
				street.Replace("дорога", "дор.");
			else if (street.Contains("канал"))
				street.Replace("канал", "кан.");
			else if (street.Contains("набережная"))
				street.Replace("набережная", "наб.");
			else if (street.Contains("переулок"))
				street.Replace("переулок", "пер.");
			else if (street.Contains("площадь"))
				street.Replace("площадь", "пл.");

			//Сокращаем пояснительное слово
			if (street.Contains("Большой"))
				street.Replace("Большой", "Б.");
			else if (street.Contains("Большая"))
				street.Replace("Большая", "Б.");
			else if (street.Contains("Верхний"))
				street.Replace("Верхний", "Верх.");
			else if (street.Contains("Верхняя"))
				street.Replace("Верхняя", "Верх.");
			else if (street.Contains("Малый"))
				street.Replace("Малый", "М.");
			else if (street.Contains("Малая"))
				street.Replace("Малая", "М.");
			else if (street.Contains("Новый"))
				street.Replace("Новый", "Н.");
			else if (street.Contains("Новая"))
				street.Replace("Новая", "Н.");
			else if (street.Contains("Нижний"))
				street.Replace("Нижний", "Нижн.");
			else if (street.Contains("Нижняя"))
				street.Replace("Нижняя", "Нижн.");
			else if (street.Contains("Средний"))
				street.Replace("Средний", "Ср.");
			else if (street.Contains("Средняя"))
				street.Replace("Средняя", "Ср.");
			else if (street.Contains("Старый"))
				street.Replace("Старый", "Ст.");
			else if (street.Contains("Старая"))
				street.Replace("Старая", "Ст.");
			else if (street.Contains("Восточный"))
				street.Replace("Восточный", "Вост.");
			else if (street.Contains("Восточная"))
				street.Replace("Восточная", "Вост.");
			else if (street.Contains("Южный"))
				street.Replace("Южный", "Южн.");
			else if (street.Contains("Южная"))
				street.Replace("Южная", "Южн.");
			else if (street.Contains("Западный"))
				street.Replace("Западный", "Зап.");
			else if (street.Contains("Западная"))
				street.Replace("Западная", "Зап.");
			else if (street.Contains("Северный"))
				street.Replace("Северный", "Сев.");
			else if (street.Contains("Северная"))
				street.Replace("Северная", "Сев.");

			//Сокращаем слова в основной части
			if (street.Contains("Братьев"))
				street.Replace("Братьев", "Бр.");
			else if (street.Contains("Красного"))
				street.Replace("Красного", "Кр.");
			else if (street.Contains("Красной"))
				street.Replace("Красной", "Кр.");
			else if (street.Contains("Красных"))
				street.Replace("Красных", "Кр.");

			return street;
		}

	}
}

