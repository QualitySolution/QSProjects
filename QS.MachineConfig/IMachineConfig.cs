using System;
using Nini.Config;

namespace QSMachineConfig
{
	[Obsolete("Переходите на интерфейс QS.Configuration.IChangeableConfiguration который абстрагирует работу с конфигурацией от конкретной реализации и библиотеки Nini.")]
	public interface IMachineConfig
    {
        IniConfigSource ConfigSource { get; set; }
        string ConfigFileName { get; set; }
        string FullConfigPath { get; }

        void ReloadConfigFile();
        void SaveConfigFile();
    }
}
