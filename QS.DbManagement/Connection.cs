using System;
using System.Collections.Generic;
using ReactiveUI;

namespace QS.DbManagement {
	public class Connection : ReactiveObject, ICloneable {

		string connectionTitle;
		public string ConnectionTitle {
			get => connectionTitle;
			set => this.RaiseAndSetIfChanged(ref connectionTitle, value);
		}

		public ConnectionTypeBase ConnectionType { get; }
		
		public List<ConnectionParameterValue> CustomParameters { get; } = new List<ConnectionParameterValue>();

		public bool Last { get; set; } = false;

		public Connection(ConnectionTypeBase connectionType, IDictionary<string, string> parameters) {
			ConnectionType = connectionType;
			ConnectionTitle = parameters["Title"];
			Last = parameters.ContainsKey("Last") && parameters["Last"] == "true";
			foreach(var parameter in ConnectionType.Parameters) {
				if(parameters.TryGetValue(parameter.Name, out var parameterValue))
					CustomParameters.Add(new ConnectionParameterValue(parameter, parameterValue));
			}
		}
		
		public bool CanConnect() => ConnectionType.CanConnect(CustomParameters);
		
		public IDbProvider CreateProvider(string password) => ConnectionType.CreateProvider(CustomParameters, password);
		public Dictionary<string, string> GetConfigDefinitions() {
			var config = new Dictionary<string, string> {
				{"Type", ConnectionType.ConnectionTypeName},
				{"Title", ConnectionTitle},
				{"Last", Last.ToString()}
			};
			foreach(var parameter in CustomParameters)
				config.Add(parameter.Name, parameter.Value?.ToString());
			return config;
		}

		public object Clone() {
			var clone = new Connection(ConnectionType, new Dictionary<string, string>()) {
				ConnectionTitle = ConnectionTitle,
				Last = Last
			};
			foreach(var parameter in CustomParameters)
				clone.CustomParameters.Add((ConnectionParameterValue)parameter.Clone());
			return clone;
		}
	}
}
