using System;
using Nini.Config;

namespace QSMachineConfig
{
	public static class MachineConfig
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static IniConfigSource ConfigSource;
		public static string ConfigFileName;

		public static void ReloadConfigFile()
		{
			string configfile = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), ConfigFileName);

			ConfigSource = new IniConfigSource (configfile);

			if(System.IO.File.Exists (configfile))
			{
				ConfigSource.Reload ();
			}
			else
				logger.Warn ("Конфигурационный фаил {0} не найден. Конфигурация не загружена.");	
		}

		public static void SaveConfigFile()
		{
			ConfigSource.Save ();
		}
	}
}

