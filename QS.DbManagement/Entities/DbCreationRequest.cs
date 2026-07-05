using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Project.Versioning;
using System.Threading;

namespace QS.DbManagement.Entities {
	/// <summary>
	/// запрос на создание базы с наполнением из скрипта
	/// </summary>
	public sealed class DbCreationRequest {
		public string DbName { get; set; }
		public string DbTitle { get; set; }

		public IDbCreatorModel CreationModel { get; set; }

		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public IApplicationInfo ApplicationInfo { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
