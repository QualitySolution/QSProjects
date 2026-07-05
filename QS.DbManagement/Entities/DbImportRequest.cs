using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Project.Versioning;
using System.Threading;

namespace QS.DbManagement.Entities {
	/// <summary>
	/// запрос на создание базы с наполнением из пользовательского дампа
	/// </summary>
	public sealed class DbImportRequest {
		public string DbName { get; set; }
		public string DbTitle { get; set; }

		/// <summary>Путь к дампу, которым наполняется база</summary>
		public string DumpFilePath { get; set; }

		/// <summary>Сервис заливки дампа из DI; строку подключения ему выдаст провайдер</summary>
		public IDbDumpService DumpService { get; set; }

		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public IApplicationInfo ApplicationInfo { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
