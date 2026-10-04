using System;
using System.Linq;

namespace QS.Utilities.Reflection
{
	public static class AttributeUtil
	{
		/// <summary>
		/// Возвращает первый атрибут указанного типа у класса или null.
		/// </summary>
		public static TAttribute GetAttribute<TAttribute>(this Type clazz, bool inherit)
			where TAttribute : Attribute
		{
			return clazz.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>().FirstOrDefault();
		}
	}
}
