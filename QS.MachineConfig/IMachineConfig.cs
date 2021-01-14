using Nini.Config;

namespace QSMachineConfig
{
    public interface IMachineConfig
    {
        IniConfigSource ConfigSource { get; set; }
        string ConfigFileName { get; set; }
        string FullConfigPath { get; }

        void ReloadConfigFile();
        void SaveConfigFile();
    }
}