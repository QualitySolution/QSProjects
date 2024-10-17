using System;
using QS.DbManagement.Responces;

namespace QS.Launcher.AppRunner {
	public class InProcessRunner : IAppRunner {

		public Action<LoginToDatabaseResponse> OnLogin;
		public void Run(LoginToDatabaseResponse loginToDatabase) {
			OnLogin?.Invoke(loginToDatabase);
		}
	}
}
