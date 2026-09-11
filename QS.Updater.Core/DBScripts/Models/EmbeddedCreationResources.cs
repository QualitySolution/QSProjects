using QS.DBScripts.Controllers;

namespace QS.DBScripts.Models {
	public class EmbeddedCreationResources : DbCreationResources {
		public CreationScript Script { get; set; }
	}
}
