using System;
using ReactiveUI;

namespace QS.DbManagement {
	public class ConnectionParameterValue : ReactiveObject {
		private readonly ConnectionParameter parameter;

		public string Title => parameter.Title;
		public string Name => parameter.Name;

		string val;
		public string Value {
			get => val;
			set => this.RaiseAndSetIfChanged(ref val, value);
		}

		public ConnectionParameterValue(ConnectionParameter parameter, string value) {
			this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
			Value = value;
		}
		
		public object Clone() => new ConnectionParameterValue(parameter, Value);
	}
}
