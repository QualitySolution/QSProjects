using System;
using System.Diagnostics;
using QS.DbManagement.Responces;

namespace QS.Launcher.AppRunner {
	public class NewProcessRunner : IAppRunner {
		private readonly LauncherOptions launcherOptions;

		public NewProcessRunner(LauncherOptions launcherOptions) {
			this.launcherOptions = launcherOptions ?? throw new ArgumentNullException(nameof(launcherOptions));
		}

		public void Run(LoginToDatabaseResponse loginToDatabase) {
			string fileName = launcherOptions.AppExecutablePath;

			Environment.SetEnvironmentVariable("QS_CONNECTION_STRING", loginToDatabase.ConnectionString, EnvironmentVariableTarget.User);
			Environment.SetEnvironmentVariable("QS_LOGIN", loginToDatabase.Login, EnvironmentVariableTarget.User);
			foreach(var par in loginToDatabase.Parameters)
				Environment.SetEnvironmentVariable("QS_" + par.Name, par.Value, EnvironmentVariableTarget.User);

			Process.Start(new ProcessStartInfo {
				WorkingDirectory = "D:\\",
				FileName = fileName,
				UseShellExecute = true,
				CreateNoWindow = true,
				Arguments = loginToDatabase.ConnectionString
			});
		}
	}
}
