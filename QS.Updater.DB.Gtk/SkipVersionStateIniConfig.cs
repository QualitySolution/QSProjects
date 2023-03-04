using System;
using QSMachineConfig;

namespace QS.Updater
{
	public class SkipVersionStateIniConfig : ISkipVersionState
	{

		public bool IsSkipedVersion(string version)
		{
			if (MachineConfig.ConfigSource.Configs["AppUpdater"] != null)
			{
				return MachineConfig.ConfigSource.Configs["AppUpdater"].Get("SkipVersion", String.Empty) == version;
			}

			return false;
		}

		public void SaveSkipVersion(string version)
		{
			#region Удаляем старый конфиг если он есть
			var oldConfig = MachineConfig.ConfigSource.Configs["Updater"];
			if (oldConfig != null)
				MachineConfig.ConfigSource.Configs.Remove(oldConfig);
			#endregion

			if (MachineConfig.ConfigSource.Configs["AppUpdater"] == null)
				MachineConfig.ConfigSource.AddConfig("AppUpdater");
			MachineConfig.ConfigSource.Configs["AppUpdater"].Set("SkipVersion", version);
			MachineConfig.ConfigSource.Save();
		}
	}
}
