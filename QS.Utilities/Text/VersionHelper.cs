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
		
		/// <summary>
		/// Метод собирает версиею из компонентов и преобразует в строковой формат, где первые две цифры версии обязательны даже если они 0.
		/// Вторая и третья только при условии больше 0.
		/// </summary>
		public static string VersionToShortString(int major, int minor, int? build = null, int? revision = null)
		{
			var ver = new Version(major, minor, build ?? 0, revision ?? 0);
			return ver.ToString(ver.Revision <= 0 ? (ver.Build <= 0 ? 2 : 3) : 4);
		}
		
		/// <summary>
		/// Метод собирает версиею из компонентов и преобразует в строковой формат, где первые две цифры версии обязательны даже если они 0.
		/// Вторая и третья только при условии больше 0.
		/// </summary>
		public static string VersionToShortString(uint major, uint minor, uint? build = null, uint? revision = null)
		{
			var ver = new Version((int)major, (int)minor, (int?)build ?? 0, (int?)revision ?? 0);
			return ver.ToString(ver.Revision <= 0 ? (ver.Build <= 0 ? 2 : 3) : 4);
		}
	}
}
