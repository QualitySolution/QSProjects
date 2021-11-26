﻿using System;
using Nini.Config;

namespace QS.MachineConfig
{
	[Obsolete("Переходите на интерфейс IChangeableConfiguration который абстрагирует работу с конфигурацие от конкретной реализации и библиотеки Nini.")]
	public static class MachineConfig
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public static IniConfigSource ConfigSource;
		public static string ConfigFileName;

		public static string FullConfigPath => System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), ConfigFileName);

		public static void ReloadConfigFile()
		{
			if(System.IO.File.Exists (FullConfigPath))
			{
				ConfigSource = new IniConfigSource (FullConfigPath);
				ConfigSource.Reload ();
			}
			else
			{
				logger.Warn ("Конфигурационный фаил {0} не найден. Конфигурация не загружена.", FullConfigPath);
				ConfigSource = new IniConfigSource ();
				ConfigSource.Save (FullConfigPath);
			}
		}

		public static void SaveConfigFile()
		{
			ConfigSource.Save (FullConfigPath);
		}
	}
}

