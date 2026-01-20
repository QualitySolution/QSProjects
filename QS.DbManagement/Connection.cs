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
		
		public int? LastBaseId { get; set; }

		public Connection(ConnectionTypeBase connectionType, IDictionary<string, string> parameters) {
			ConnectionType = connectionType;
			ConnectionTitle = parameters["Title"];
			Last = parameters.ContainsKey("Last") && parameters["Last"] == "True";
			if(parameters.ContainsKey("LastBaseId") && int.TryParse(parameters["LastBaseId"], out int lastBaseId))
				LastBaseId = lastBaseId;
			foreach(var parameter in ConnectionType.Parameters)
				CustomParameters.Add(new ConnectionParameterValue(parameter, parameters.ContainsKey(parameter.Name) ? parameters[parameter.Name] : null));
		}

		public Connection(Connection other) {
			ConnectionType = other.ConnectionType;
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
