using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DBScripts.Models {
	public class MySqlCreationResources : DbCreationResources {
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interactions { get; set; }
		public CreationScript Script { get; set; }
	}
}
