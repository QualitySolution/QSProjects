using QS.DbManagement.Entities;

namespace QS.Launcher.AppRunner {
	public interface IAppRunner {
		void Run(LoginToDatabaseResponse loginToDatabase);
	}
}
