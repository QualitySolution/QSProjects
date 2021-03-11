using System;
using System.IO;
using Nini.Config;

namespace QS.Configuration
{
	public class IniFileConfiguration : IChangeableConfiguration
	{
		private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private IniConfigSource configSource;

		public readonly string IniFile;

		public IniFileConfiguration(string iniFile)
		{
			if(string.IsNullOrWhiteSpace(iniFile)) {
				throw new ArgumentException("Имя ini файла должно быть указано.", nameof(iniFile));
			}
			this.IniFile = iniFile;

			if(File.Exists(iniFile)) {
				configSource = new IniConfigSource(iniFile);
				configSource.Reload();
			}
			else {
				logger.Warn("Конфигурационный фаил {0} не найден. Создан новый.", iniFile);
				configSource = new IniConfigSource();
				configSource.Save(iniFile);
			}

			configSource.AutoSave = true;
		}

		/// <summary>
		/// Получаем или устанавливаем элемент. В ключе секцию указываем через ":"
		/// Например "Секция:параметер"
		/// </summary>
		/// <value>The item.</value>
		public string this[string key] {
			get {
				ParseKey(key, out string section, out string parameter);
				return configSource.Configs[section]?.Get(parameter);
			}
			set {
				ParseKey(key, out string section, out string parameter);
				if(configSource.Configs[section] == null && value != null)
					configSource.AddConfig(section);
				if(value != null)
					configSource.Configs[section].Set(parameter, value);
				if(value == null && configSource.Configs[section]?.Contains(parameter) == true) {
					configSource.Configs[section].Remove(parameter);
					if(configSource.Configs[section].GetKeys().Length == 0) {
						configSource.Configs.Remove((IConfig)configSource.Configs[section]);
						configSource.Save();
					}
				}
			}
		}

		public void Reload()
		{
			configSource.Reload();
		}

		#region private
		private void ParseKey(string key, out string section, out string parameter)
		{
			section = null;
			parameter = null;

			var indexСolon = key.IndexOf(':');
			if(indexСolon > 0) {
				section = key.Substring(0, indexСolon);
				parameter = key.Substring(indexСolon + 1);
			}
			else
				parameter = key;
		}
		#endregion
	}
}
