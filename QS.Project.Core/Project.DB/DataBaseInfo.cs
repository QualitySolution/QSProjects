using System;
using QS.BaseParameters;

namespace QS.Project.DB {
	public class DataBaseInfo : IDataBaseInfo {
		private readonly ParametersService parameters;

		public DataBaseInfo(string name, ParametersService parameters, bool isDemo = false) {
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Не указано имя базы данных.", nameof(name));
			Name = name;
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			IsDemo = isDemo;
		}

		public string Name { get; }
		public bool IsDemo { get; }

		public Guid? BaseGuid {
			get {
				if(!parameters.All.TryGetValue("BaseGuid", out var value) || string.IsNullOrWhiteSpace(value))
					return null;
				if(Guid.TryParse(value, out var guid))
					return guid;
				throw new InvalidOperationException($"В базе данных «{Name}» параметр BaseGuid содержит некорректный GUID: «{value}».");
			}
		}

		// ParametersService перечитывает параметры после миграции: не сохраняем отдельную копию версии.
		public Version Version {
			get {
				if(!parameters.All.TryGetValue("version", out var value) || string.IsNullOrWhiteSpace(value))
					throw new InvalidOperationException($"В базе данных «{Name}» отсутствует обязательный параметр version.");
				if(System.Version.TryParse(value, out var version))
					return version;
				throw new InvalidOperationException($"В базе данных «{Name}» параметр version содержит некорректную версию: «{value}».");
			}
		}
	}
}
