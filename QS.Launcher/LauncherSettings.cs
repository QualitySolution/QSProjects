using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using QS.Configuration;

namespace QS.Launcher {
	/// <summary>Настройки лаунчера в JSON. Ключи вложенных разделов разделяются двоеточием.</summary>
	public sealed class LauncherSettings : IChangeableConfiguration {
		private readonly string path;
		private JsonObject values = new JsonObject();

		public LauncherSettings(string path) {
			this.path = path ?? throw new ArgumentNullException(nameof(path));
		}

		public string this[string key] {
			get {
				JsonNode node = values;
				foreach(var part in key.Split(':'))
					node = node is JsonObject obj ? obj[part] : null;
				return node?.GetValue<string>();
			}
			set {
				var parts = key.Split(':');
				var obj = values;
				for(var i = 0; i < parts.Length - 1; i++) {
					var child = obj[parts[i]] as JsonObject;
					if(child == null) {
						child = new JsonObject();
						obj[parts[i]] = child;
					}
					obj = child;
				}
				obj[parts[parts.Length - 1]] = value;
				var directory = Path.GetDirectoryName(path);
				if(!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
				var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
				try {
					File.WriteAllText(temp, values.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
					if(File.Exists(path)) File.Replace(temp, path, null);
					else File.Move(temp, path);
				}
				finally {
					if(File.Exists(temp)) File.Delete(temp);
				}
			}
		}

		public void Reload() {
			if(File.Exists(path))
				values = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
					?? throw new InvalidDataException("Файл настроек лаунчера должен содержать JSON-объект.");
			else values = new JsonObject();
		}
	}
}
