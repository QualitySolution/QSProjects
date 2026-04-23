using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace QS.DbManagement {
	public class Connection : ReactiveObject, ICloneable {

		string connectionTitle;
		public string ConnectionTitle {
			get => connectionTitle;
			set => this.RaiseAndSetIfChanged(ref connectionTitle, value);
		}

		ConnectionTypeBase connectionType;
		public ConnectionTypeBase ConnectionType {
			get => connectionType;
			set {
				if(connectionType == value)
					return;
				// Сохраняем значения параметров, чтобы перенести те, что имеют общие имена
				var previousValues = CustomParameters.ToDictionary(p => p.Name, p => p.Value);
				this.RaiseAndSetIfChanged(ref connectionType, value);
				var newParameters = new List<ConnectionParameterValue>();
				if(connectionType != null) {
					foreach(var parameter in connectionType.Parameters) {
						previousValues.TryGetValue(parameter.Name, out var carriedValue);
						newParameters.Add(new ConnectionParameterValue(parameter, carriedValue));
					}
				}
				// Меняем ссылку целиком, чтобы привязкиW перечитали список
				CustomParameters = newParameters;
			}
		}

		List<ConnectionParameterValue> customParameters = new List<ConnectionParameterValue>();
		public List<ConnectionParameterValue> CustomParameters {
			get => customParameters;
			private set => this.RaiseAndSetIfChanged(ref customParameters, value);
		}

		public bool Last { get; set; } = false;

		public int? LastBaseId { get; set; }

		public Connection(ConnectionTypeBase connectionType, IDictionary<string, string> parameters) {
			this.connectionType = connectionType;
			ConnectionTitle = parameters["Title"];
			Last = parameters.ContainsKey("Last") && parameters["Last"] == "True";
			if(parameters.ContainsKey("LastBaseId") && int.TryParse(parameters["LastBaseId"], out int lastBaseId))
				LastBaseId = lastBaseId;
			foreach(var parameter in ConnectionType.Parameters)
				CustomParameters.Add(new ConnectionParameterValue(parameter, parameters.ContainsKey(parameter.Name) ? parameters[parameter.Name] : null));
		}

		public Connection(Connection other) {
			connectionType = other.ConnectionType;
			ConnectionTitle = other.ConnectionTitle;
			foreach(var parameter in other.CustomParameters)
				CustomParameters.Add(parameter.Clone() as ConnectionParameterValue);
		}

		public bool CanConnect() => ConnectionType.CanConnect(CustomParameters);

		public IDbProvider CreateProvider(string password) => ConnectionType.CreateProvider(CustomParameters, password);
		public Dictionary<string, string> GetConfigDefinitions() {
			var config = new Dictionary<string, string> {
				{"Type", ConnectionType.ConnectionTypeName},
				{"Title", ConnectionTitle},
				{"Last", Last.ToString()}
			};
			if(LastBaseId.HasValue)
				config.Add("LastBaseId", LastBaseId.Value.ToString());
			foreach(var parameter in CustomParameters)
				config.Add(parameter.Name, parameter.Value?.ToString());
			return config;
		}

		public object Clone() => new Connection(this);
	}
}
