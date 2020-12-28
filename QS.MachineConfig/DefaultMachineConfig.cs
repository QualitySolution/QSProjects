﻿using System;
using System.IO;
using Nini.Config;

namespace QSMachineConfig
{
    public class DefaultMachineConfig : IMachineConfig
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IniConfigSource ConfigSource { get; set; }
        public string ConfigFileName { get; set; }
        public string FullConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFileName);

        public void ReloadConfigFile()
        {
            if(File.Exists(FullConfigPath)) {
                ConfigSource = new IniConfigSource(FullConfigPath);
                ConfigSource.Reload();
            }
            else {
                logger.Warn("Конфигурационный фаил {0} не найден. Конфигурация не загружена.", FullConfigPath);
                ConfigSource = new IniConfigSource();
                ConfigSource.Save(FullConfigPath);
            }
        }

        public void SaveConfigFile()
        {
            ConfigSource.Save(FullConfigPath);
        }
    }
}