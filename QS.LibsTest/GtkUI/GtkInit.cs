using System;
using Gtk;

namespace QS.Test.GtkUI
{
	public static class GtkInit
	{
		private static bool initialized;

		/// <summary>
		/// Внимание! Для запуска таких тестов на дженкисе(то есть без наличия запущенного XServer)
		/// тесты необходимо запускать в виртуальном дисплее. Например так:
		/// <code>xvfb-run mono QSProjects/packages/NUnit.ConsoleRunner.3.10.0/tools/nunit3-console.exe QSProjects/QS.LibsTest/bin/Debug/QS.LibsTest.dll </code>
		/// Естественно в системе должен быть установлен xvfb-run.s
		/// </summary>
		public static void AtOnceInitGtk()
		{
			if (initialized)
				return;

			Application.Init();
			initialized = true;
		}
	}
}
