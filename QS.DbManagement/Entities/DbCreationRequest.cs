using System.Threading;
using QS.DbManagement.Creation;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Project.Versioning;

namespace QS.DbManagement.Entities {
	/// <summary>
	/// запрос на создание базы
	/// </summary>
	public sealed class DbCreationRequest<CreationArgs> where CreationArgs : DbCreationResources {
		public string DbName { get; set; }
		public string DbTitle { get; set; }

		public IDbCreatorInteraction Interaction { get; set; }

		/// <summary>Чем наполнять созданную базу</summary>
		public DbCreationFactory CreationFactory { get; set; }
		public IApplicationInfo ApplicationInfo { get; set; }

		public CreationArgs CreationResources { get; set; }
	}
}
