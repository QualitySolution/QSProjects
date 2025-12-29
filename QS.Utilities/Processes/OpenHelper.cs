using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QS.Utilities.Processes {
	/// <summary>
	/// Помощник запуска процессов. Или открытия ссылок и файлов.
	/// </summary>
	public static class OpenHelper {
		/// <summary>
		/// Открывает URL в браузере по умолчанию.
		/// Работает на всех платформах.
		/// </summary>
		public static void OpenUrl(string url)
		{
			try
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					Console.WriteLine("Неизвестная ОС");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка: {ex.Message}");
			}
		}
	}
}
