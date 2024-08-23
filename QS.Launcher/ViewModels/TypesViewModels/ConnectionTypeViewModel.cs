using Avalonia.Media.Imaging;
using System.Collections.Generic;
using QS.DbManagement;

namespace QS.Launcher.ViewModels.TypesViewModels;

public class ConnectionTypeViewModel
{
	public string Title { get; set; } = null!;

	public List<ParameterVM>? Parameters { get; set; }

	public Bitmap? Icon { get; set; }
}
