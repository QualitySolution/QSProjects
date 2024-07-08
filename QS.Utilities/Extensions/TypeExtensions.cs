using System;
using System.Linq;
using System.Reflection;

namespace QS.Utilities.Extensions {
	public static class TypeExtensions {
		/// <summary>
		/// Получение информации о свойстве<br/>
		/// Получает информацию о свойстве, с приоритетом из указанного типа, по отношению к родительским классам<br/>
		/// Отрабатывает при наличии new у перезаписываемого свойства с измененным типом у наследника
		/// </summary>
		/// <param name="mappedClass">Подключенный к БД класс</param>
		/// <param name="propName">Имя свойства</param>
		/// <returns></returns>
		public static PropertyInfo GetPropertyInfo(this Type mappedClass, string propName) {
			return mappedClass
				.GetProperties()
				.Where(x => x.Name == propName)
				.OrderByDescending(x => x.DeclaringType == mappedClass)
				.FirstOrDefault();
		}
	}
}
