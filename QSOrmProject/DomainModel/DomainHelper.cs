using System;

namespace QSOrmProject
{
	public static class DomainHelper
	{
		public static string GetObjectTilte(object value)
		{
			var prop = value.GetType ().GetProperty ("Title");
			if (prop != null) {
				return prop.GetValue (value, null).ToString();
			}

			prop = value.GetType ().GetProperty ("Name");
			if (prop != null) {
				return prop.GetValue (value, null).ToString();
			}

			return value.ToString ();
		}

		public static int GetId(object value)
		{
			if (value == null)
				throw new ArgumentNullException ();
			if (value is IDomainObject)
				return (value as IDomainObject).Id;

			var prop = value.GetType ().GetProperty ("Id");
			if (prop == null)
				throw new ArgumentException ("Для работы метода тип {0}, должен иметь свойство Id.");

			return (int)prop.GetValue (value, null);
		}

	}
}

