using QS.DbManagement;
using ReactiveUI;

namespace QS.Launcher.ViewModels.ModelsDTO {
	public class ConnectionParameterDTO : ReactiveObject {

		public string Title { get; }

		object val;
		public object Value {
			get => val;
			set => this.RaiseAndSetIfChanged(ref val, value);
		}

		public ConnectionParameterDTO(ConnectionParameter parameter) {
			Title = parameter.Title;
			Value = parameter.Value;
		}
	}
}
