using System.Collections.Generic;

namespace QS.Project.Services.FileDialog
{
    public interface IDialogResult
    {
		bool Successful { get; }
        string Path { get; }
		IEnumerable<string> Paths { get; }
    }
}
