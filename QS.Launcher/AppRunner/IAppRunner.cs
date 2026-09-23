using QS.DbManagement.Responces;
using System.Collections.Generic;

namespace QS.Launcher.AppRunner {
	public interface IAppRunner {
		void Run(LoginToDatabaseResponse loginToDatabase);
	}

	/// <summary>
	/// Реализуется раннерами, которые позволяют пользователю выбрать один из
	/// нескольких установленных вариантов приложения.
	/// </summary>
	public interface IMultipleAppRunner : IAppRunner {
		IReadOnlyList<ApplicationRunOption> Applications { get; }
		ApplicationRunOption SelectedApplication { get; set; }
	}
}
