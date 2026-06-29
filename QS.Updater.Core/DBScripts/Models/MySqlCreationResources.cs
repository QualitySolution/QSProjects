using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace QS.DbManagement.Creation {

	public class MySqlCreationResources : DbCreationResources {
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interactions { get; set; }
		public CreationScript Script { get; set; }
	}
}
