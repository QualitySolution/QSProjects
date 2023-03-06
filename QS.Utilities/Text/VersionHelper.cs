using System;
namespace QS.Utilities.Text
{
	public static class VersionHelper
	{
		/// <summary>
		/// Преобразуем версию в строковое представление,  где первые две цифры версии обязательны даже если они 0.
		/// Вторая и третья только при условии больше 0.
		/// </summary>
		public static string VersionToShortString(this Version version)
		{
			return version.ToString(version.Revision <= 0 ? (version.Build <= 0 ? 2 : 3) : 4);
		}

		/// <summary>
		/// Метод преобразует строку с версией в формат, где первые две цифры версии обязательны даже если они 0.
		/// Вторая и третья только при условии больше 0.
		/// </summary>
		public static string VersionToShortString(this string version)
		{
			var ver = Version.Parse(version);
			return ver.ToString(ver.Revision <= 0 ? (ver.Build <= 0 ? 2 : 3) : 4);
		}
	}
}
