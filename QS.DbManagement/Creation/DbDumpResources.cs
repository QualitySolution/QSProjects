using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DbManagement.Creation {
	public class DbDumpResources : DbCreationResources {
		public string DumpFilePath { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interactions { get; set; }
		public CreationScript Script { get; set; }
	}
}
