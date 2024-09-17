using IniParser.Model;
using IniParser;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace QS.Configuration {
	public class IniFileMSConfiguration : IChangeableConfiguration, IConfiguration
	{
		private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly FileIniDataParser parser = new FileIniDataParser();
		private IniData config;

		public readonly string IniFile;

		public IniFileMSConfiguration(string iniFile) {
			if(string.IsNullOrWhiteSpace(iniFile)) {
				throw new ArgumentException("Имя ini файла должно быть указано.", nameof(iniFile));
			}
			IniFile = iniFile;

			Reload();
			config.Configuration.NewLineStr = "\r\n"; //Устанавливаем виндовое окончание строк, так как старый вариант на всех системах сохранял так.
		}

		/// <summary>
		/// Получаем или устанавливаем элемент. В ключе секцию указываем через ":"
		/// Например "Секция:параметр"
		/// </summary>
		/// <value>The item.</value>
		public string this[string key] {
			get {
				ParseKey(key, out string section, out string parameter);
				if(!config.Sections.ContainsSection(section))
					return null;
				return config[section][parameter];
			}
			set {
				ParseKey(key, out string section, out string parameter);
				if(value != null) {
					if(config[section][parameter] != value) {
						config[section][parameter] = value;
						parser.WriteFile(IniFile, config);
					}
				}
				else {
					if(config.Sections.ContainsSection(section)) {
						if(String.IsNullOrWhiteSpace(parameter)) {
							config.Sections.RemoveSection(section);
							parser.WriteFile(IniFile, config);
						}
						else if(config[section].ContainsKey(parameter)) {
							config[section].RemoveKey(parameter);
							if(config[section].Count == 0) {
								config.Sections.RemoveSection(section);
							}
							parser.WriteFile(IniFile, config);
						}
					}
				}
			}
		}

		public void Reload() {
			if(File.Exists(IniFile)) {
				config = parser.ReadFile(IniFile, Encoding.UTF8);
			}
			else {
				logger.Warn("Конфигурационный файл {0} не найден.", IniFile);
				config = new IniData();
			}
		}

		#region private
		private void ParseKey(string key, out string section, out string parameter) {
			section = null;
			parameter = null;

			var index = key.IndexOf(':');
			if(index > 0) {
				section = key.Substring(0, index);
				parameter = key.Substring(index + 1);
			}
			else
				parameter = key;
		}

		public IConfigurationSection GetSection(string key) {
			throw new NotImplementedException();
		}

		public IEnumerable<IConfigurationSection> GetChildren() {
			throw new NotImplementedException();
		}

		public IChangeToken GetReloadToken() {
			throw new NotImplementedException();
		}
		#endregion
	}
}
