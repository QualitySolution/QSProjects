using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DbManagement.Creation {
	public class DbDumpResources : DbCreationResources {
		public string DumpFilePath { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
	}
}
