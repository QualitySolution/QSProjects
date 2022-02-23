using QS.Project.Services.FileDialog;
using System.Collections.Generic;
using System.Linq;

namespace QS.Dialog.GtkUI.FileDialog
{
    public class DialogResult : IDialogResult
	{
		internal DialogResult(string path, IEnumerable<string> paths)
		{
			Successful = true;
			Path = path;
			Paths = paths;
		}

		internal DialogResult()
		{
			Successful = false;
			Path = null;
			Paths = Enumerable.Empty<string>();
		}

		public bool Successful { get; }

		public string Path { get; }
		public IEnumerable<string> Paths { get; }
	}
}
