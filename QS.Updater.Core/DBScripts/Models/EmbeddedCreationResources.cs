using QS.DBScripts.Controllers;
using QS.Dialog;

namespace QS.DBScripts.Models {
	public class EmbeddedCreationResources : DbCreationResources {
		public CreationScript Script { get; set; }
	}
}
