using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Utilities.Enums
{
	public static class EnumHelper
	{
		/// <summary>
		/// Получает список значений Enum-а.
		/// </summary>
		/// <typeparam name="TEnum">Тип перечисления</typeparam>
		/// <returns>Список значений</returns>
		public static IList<TEnum> GetValuesList<TEnum>() where TEnum : Enum
		{
			return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
		}
	}
}
