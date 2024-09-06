using QS.DbManagement;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.Launcher.ViewModels.ModelsDTO;
internal class ConnectionParameterDTO : ReactiveObject {

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
