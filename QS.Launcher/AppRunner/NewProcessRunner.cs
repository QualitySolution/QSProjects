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
			if (!File.Exists(this.exeFileName))
				throw new ArgumentException($"Запускаемого файла {exeFileName} не существует.");

			var startInfo = new ProcessStartInfo {
				WorkingDirectory = Path.GetDirectoryName(exeFileName),
				FileName = Path.GetFullPath(exeFileName),
				UseShellExecute = false,
				CreateNoWindow = true
			};

			startInfo.Environment["QS_CONNECTION_STRING"] = loginToDatabase.ConnectionString;
			startInfo.Environment["QS_LOGIN"] = loginToDatabase.Login;
			foreach (var par in loginToDatabase.Parameters)
				startInfo.Environment["QS_" + par.Key] = par.Value;

			Process.Start(startInfo);
		}
	}
}
