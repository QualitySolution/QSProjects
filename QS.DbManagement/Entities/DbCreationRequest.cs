using QS.DbManagement.Creation;
using QS.DBScripts.Controllers;

namespace QS.DbManagement.Entities {
	public sealed class DbCreationRequest {
		public string DbName { get; set; }
		public string DbTitle { get; set; }

		public IDbCreatorInteraction Interaction { get; set; }

		/// <summary>Чем наполнять созданную базу</summary>
		public DbCreationFactory CreationFactory { get; set; }

		public DbCreationResources CreationResources { get; set; }
	}
}
