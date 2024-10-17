using QS.DbManagement.Responces;

namespace QS.Launcher.AppRunner {
	public interface IAppRunner {
		void Run(LoginToDatabaseResponse loginToDatabase);
	}
}
