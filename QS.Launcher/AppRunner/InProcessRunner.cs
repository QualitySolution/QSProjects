using System;
using QS.Dialog;
using QS.DbManagement.Responces;

namespace QS.Launcher.AppRunner {
	public class InProcessRunner : IAppRunner {

		public Action<LoginToDatabaseResponse> OnLogin;

		/// <summary>
		/// Прогресс запуска, который лаунчер показывает пользователю, пока приложение готовит главное окно.
		/// </summary>
		public IProgressBarDisplayable Progress { get; set; }

		public void Run(LoginToDatabaseResponse loginToDatabase) {
			OnLogin?.Invoke(loginToDatabase);
		}
	}
}
