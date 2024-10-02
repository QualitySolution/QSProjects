using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.Launcher;
public class LauncherOptions {
	public Uri CompanyImage { get; set; }

	public Uri CompanyIcon { get; set; }

	public string AppTitle { get; set; }

	public string AppExecutablePath { get; set; }

	public string? OldConfigFilename { get; set; }
}
