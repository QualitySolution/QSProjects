using System;
using QS.Osm.DTO;

namespace QS.Osm
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
				street = street.Replace("улица", "ул.");
			else if (street.Contains("проспект"))
				street = street.Replace("проспект", "пр.");
			else if (street.Contains("бульвар"))
				street = street.Replace("бульвар", "бульв.");
			else if (street.Contains("дорога"))
				street = street.Replace("дорога", "дор.");
			else if (street.Contains("канал"))
				street = street.Replace("канал", "кан.");
			else if (street.Contains("набережная"))
				street = street.Replace("набережная", "наб.");
			else if (street.Contains("переулок"))
				street = street.Replace("переулок", "пер.");
			else if (street.Contains("площадь"))
				street = street.Replace("площадь", "пл.");

			//Сокращаем пояснительное слово
			if (street.Contains("Большой"))
				street = street.Replace("Большой", "Б.");
			else if (street.Contains("Большая"))
				street = street.Replace("Большая", "Б.");
			else if (street.Contains("Верхний"))
				street = street.Replace("Верхний", "Верх.");
			else if (street.Contains("Верхняя"))
				street = street.Replace("Верхняя", "Верх.");
			else if (street.Contains("Малый"))
				street = street.Replace("Малый", "М.");
			else if (street.Contains("Малая"))
				street = street.Replace("Малая", "М.");
			else if (street.Contains("Новый"))
				street = street.Replace("Новый", "Н.");
			else if (street.Contains("Новая"))
				street = street.Replace("Новая", "Н.");
			else if (street.Contains("Нижний"))
				street = street.Replace("Нижний", "Нижн.");
			else if (street.Contains("Нижняя"))
				street = street.Replace("Нижняя", "Нижн.");
			else if (street.Contains("Средний"))
				street = street.Replace("Средний", "Ср.");
			else if (street.Contains("Средняя"))
				street = street.Replace("Средняя", "Ср.");
			else if (street.Contains("Старый"))
				street = street.Replace("Старый", "Ст.");
			else if (street.Contains("Старая"))
				street = street.Replace("Старая", "Ст.");
			else if (street.Contains("Восточный"))
				street = street.Replace("Восточный", "Вост.");
			else if (street.Contains("Восточная"))
				street = street.Replace("Восточная", "Вост.");
			else if (street.Contains("Южный"))
				street = street.Replace("Южный", "Южн.");
			else if (street.Contains("Южная"))
				street = street.Replace("Южная", "Южн.");
			else if (street.Contains("Западный"))
				street = street.Replace("Западный", "Зап.");
			else if (street.Contains("Западная"))
				street = street.Replace("Западная", "Зап.");
			else if (street.Contains("Северный"))
				street = street.Replace("Северный", "Сев.");
			else if (street.Contains("Северная"))
				street = street.Replace("Северная", "Сев.");

			//Сокращаем слова в основной части
			if (street.Contains("Братьев"))
				street = street.Replace("Братьев", "Бр.");
			else if (street.Contains("Красного"))
				street = street.Replace("Красного", "Кр.");
			else if (street.Contains("Красной"))
				street = street.Replace("Красной", "Кр.");
			else if (street.Contains("Красных"))
				street = street.Replace("Красных", "Кр.");

			return street;
		}

		public static LocalityType GetLocalityTypeByName (string localityName)
		{
			localityName = localityName.Trim ();
			return (LocalityType)Enum.Parse(typeof(LocalityType), localityName);
		}
	}
}

