using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace QS.Utilities.Enums
{
	public static class EnumHelper
	{
		/// <summary>
		/// Возвращает локализованное название значения перечисления из <see cref="DisplayAttribute"/>.
		/// </summary>
		public static string GetEnumTitle(this Enum value)
		{
			var field = value.GetType().GetField(value.ToString());
			return field == null ? value.ToString() : field.GetEnumTitle();
		}

		/// <summary>
		/// Возвращает локализованное название поля перечисления из <see cref="DisplayAttribute"/>.
		/// </summary>
		public static string GetEnumTitle(this FieldInfo field)
		{
			return field.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? field.Name;
		}

		/// <summary>
		/// Возвращает сокращённое локализованное название значения перечисления.
		/// </summary>
		public static string GetEnumShortTitle(this Enum value)
		{
			var field = value.GetType().GetField(value.ToString());
			return field == null ? value.ToString() : field.GetShortTitle();
		}

		/// <summary>
		/// Возвращает сокращённое локализованное название поля перечисления.
		/// </summary>
		public static string GetShortTitle(this FieldInfo field)
		{
			var display = field.GetCustomAttribute<DisplayAttribute>();
			var shortName = display?.GetShortName();
			return string.IsNullOrWhiteSpace(shortName) ? display?.GetName() ?? field.Name : shortName;
		}

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
