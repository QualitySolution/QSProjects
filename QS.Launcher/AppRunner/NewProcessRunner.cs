using System;
using System.Diagnostics;
using System.IO;
using QS.DbManagement.Responces;

namespace QS.Launcher.AppRunner {
	public class NewProcessRunner : IAppRunner {
		private readonly string exeFileName;

		public NewProcessRunner(string exeFileName) {
			this.exeFileName = exeFileName;
		}

		public void Run(LoginToDatabaseResponse loginToDatabase) {
			if(File.Exists(this.exeFileName))
				throw new ArgumentException($"Запускаемого файла {exeFileName} не существует.");

			Environment.SetEnvironmentVariable("QS_CONNECTION_STRING", loginToDatabase.ConnectionString, EnvironmentVariableTarget.User);
			Environment.SetEnvironmentVariable("QS_LOGIN", loginToDatabase.Login, EnvironmentVariableTarget.User);
			foreach(var par in loginToDatabase.Parameters)
				Environment.SetEnvironmentVariable("QS_" + par.Key, par.Value, EnvironmentVariableTarget.User);
			
			Process.Start(new ProcessStartInfo {
				WorkingDirectory = Path.GetDirectoryName(exeFileName),
				FileName = Path.GetFileName(exeFileName),
				UseShellExecute = true,
				CreateNoWindow = true,
			});
		}
	}
}
