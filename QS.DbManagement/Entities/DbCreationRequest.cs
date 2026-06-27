using System.Threading;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Project.Versioning;

namespace QS.DbManagement.Entities {
	/// <summary>
	/// запрос на создание базы
	/// </summary>
	public sealed class DbCreationRequest {
		public string DbName { get; set; }
		public string DbTitle { get; set; }

		/// <summary>Чем наполнять созданную базу</summary>
		public IDbFillStrategy FillStrategy { get; set; }
		public IApplicationInfo ApplicationInfo { get; set; }

		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
