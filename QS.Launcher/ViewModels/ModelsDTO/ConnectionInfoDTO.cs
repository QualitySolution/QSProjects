using Avalonia.Media.Imaging;
using QS.DbManagement;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace QS.Launcher.ViewModels.ModelsDTO;

internal class ConnectionInfoDTO : ReactiveObject, ICloneable {

	string title; // do this realy needs a setter?
	public string Title {
		get => title;
		set => this.RaiseAndSetIfChanged(ref title, value);
	}

	ObservableCollection<ConnectionParameterDTO> parameters;
	public ObservableCollection<ConnectionParameterDTO> Parameters {
		get => parameters;
		set => this.RaiseAndSetIfChanged(ref parameters, value);
	}

	public Bitmap IconBytes { get; }

	public ConnectionInfo Instance { get; }

	public void UpdateFields() {
		Instance.Title = Title;
		Instance.Parameters = new(Parameters.Select(pdto => new ConnectionParameter(pdto.Title, pdto.Value)));
	}

	public ConnectionInfoDTO(ConnectionInfo info) {

		if(info is null)
			return;

		Instance = info;

		Title = info.Title;
		Parameters = new(info.Parameters.Select(p => new ConnectionParameterDTO(p)));

		if (info.IconBytes != null)
			using(var stream = new System.IO.MemoryStream(info.IconBytes))
				IconBytes = new Bitmap(stream);
	}

	public ConnectionInfoDTO() {
		
	}

	public object Clone() {
		return new ConnectionInfoDTO(Instance.Clone() as ConnectionInfo);
	}
}
