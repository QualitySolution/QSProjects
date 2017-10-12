using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace QSProjectsLib
{
	public static class WindowStartupFix
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static void WindowsCheck()
		{
			if (IsWindows)
				CheckWindowsGtk();
		}

		static bool CheckWindowsGtk()
		{
			string location = null;
			Version version = null;
			Version minVersion = new Version(2, 12, 21);

			using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\InstallFolder"))
			{
				if (key != null)
					location = key.GetValue(null) as string;
			}
			using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\Version"))
			{
				if (key != null)
					Version.TryParse(key.GetValue(null) as string, out version);
			}

			//TODO: check build version of GTK# dlls in GAC
			if (version == null || version < minVersion || location == null || !File.Exists(Path.Combine(location, "bin", "libgtk-win32-2.0-0.dll")))
			{
				logger.Error("Did not find required GTK# installation");
				string url = "http://www.mono-project.com/download/#download-win";
				string caption = "Fatal Error";
				var culture = System.Globalization.CultureInfo.CurrentCulture;
				string message;
				if(culture.Name.StartsWith("ru"))
					message =
						"Программа {0} не обнаружила установки нужной версии GTK#. При нажатии на OK откроется страница " +
						"с которой вы сможете скачать и установить последнюю версию GTK#.";	
				else 
					message =
					"{0} did not find the required version of GTK#. Please click OK to open the download page, where " +
					"you can download and install the latest version.";
				if (DisplayWindowsOkCancelMessage(
					string.Format(message, Assembly.GetExecutingAssembly().GetName().Name, url), caption)
				)
				{
					Process.Start(url);
				}
				return false;
			}

			logger.Info("Found GTK# version " + version);

			var path = Path.Combine(location, @"bin");
			try
			{
				if (SetDllDirectory(path))
				{
					return true;
				}
			}
			catch (EntryPointNotFoundException)
			{
			}
			// this shouldn't happen unless something is weird in Windows
			logger.Error("Unable to set GTK+ dll directory");
			return true;
		}

		static bool DisplayWindowsOkCancelMessage(string message, string caption)
		{
			var name = typeof(int).Assembly.FullName.Replace("mscorlib", "System.Windows.Forms");
			var asm = Assembly.Load(name);
			var md = asm.GetType("System.Windows.Forms.MessageBox");
			var mbb = asm.GetType("System.Windows.Forms.MessageBoxButtons");
			var okCancel = Enum.ToObject(mbb, 1);
			var dr = asm.GetType("System.Windows.Forms.DialogResult");
			var ok = Enum.ToObject(dr, 1);

			const BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static;
			return md.InvokeMember("Show", flags, null, null, new object[] { message, caption, okCancel }).Equals(ok);
		}

		[System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		static extern bool SetDllDirectory(string lpPathName);


		public static bool IsWindows{
			get{
				return Path.DirectorySeparatorChar == '\\';
			}
		}
	}
}
