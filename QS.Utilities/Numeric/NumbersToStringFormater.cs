using System;
using System.Linq;

namespace QS.Utilities.Numeric {
	public static class NumbersToStringFormater {
		/// <summary>
		/// Преобразовывает число в двоичный формат и разбивает его на группы по 8 бит.
		/// </summary>
		public static string ToStringAsBinary32(this uint value)
		{
			// Преобразуем число в двоичный формат и дополняем до 32 бит нулями
			var binaryString = Convert.ToString(value, 2).PadLeft(32, '0');
    
			// Разбиваем строку на группы по 8 символов и соединяем с пробелами
			return string.Join(" ", Enumerable.Range(0, 4)
				.Select(i => binaryString.Substring(i * 8, 8)));
		}
	}
}
