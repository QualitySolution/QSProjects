using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace QS.MachineConfig.Configuration
{
	public class JsonConfigurationManager : IConfigurationManager
	{
		public JsonConfigurationManager(string configFileName, string configSubFolder = null)
		{
			ConfigFileName = configFileName;
			ConfigSubFolder = configSubFolder;
		}

		public string ConfigFileName { get; set; }
		public string ConfigSubFolder { get; set; }

		public string FullConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigSubFolder, ConfigFileName);
		public string ConfigFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigSubFolder);


		public IAppConfig GetConfiguration()
		{
			ThrowIfNotValidConfigFileName(ConfigFileName);
			if(!Directory.Exists(ConfigFolderPath))
			{
				Directory.CreateDirectory(ConfigFolderPath);
			}
			if(!File.Exists(FullConfigPath))
			{
				using var fs = new StreamWriter(FullConfigPath, false, Encoding.UTF8);
				fs.Write("{}");
			}
			var configurationAsString = File.ReadAllText(FullConfigPath);
			var config = JsonSerializer.Deserialize<AppConfig>(configurationAsString);
			if(config == null)
			{
				throw new InvalidOperationException("Не удалось прочитать конфигурацию из файла");
			}
			config.Connections ??= new List<Connection>();
			return config;
		}

		public void SaveConfigration(IAppConfig appConfig)
		{
			ThrowIfNotValidConfigFileName(ConfigFileName);
			var options = new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
				WriteIndented = true
			};
			File.WriteAllText(FullConfigPath, JsonSerializer.Serialize(appConfig, options), Encoding.UTF8);
		}

		private static void ThrowIfNotValidConfigFileName(string configFileName)
		{
			if(string.IsNullOrWhiteSpace(configFileName))
			{
				throw new ArgumentException("Не указано название файла конфигурации", nameof(configFileName));
			}
			if(!configFileName.EndsWith(".json"))
			{
				throw new ArgumentException("Файл конфигурации должен иметь расширение .json", nameof(configFileName));
			}
		}
	}
}
