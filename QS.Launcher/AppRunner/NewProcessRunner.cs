using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using QS.DbManagement.Responces;

namespace QS.Launcher.AppRunner {
	public class NewProcessRunner : IMultipleAppRunner {
		public NewProcessRunner(string executableFileName)
			: this(new[] { new ApplicationRunOption("Приложение", executableFileName) }) {
		}

		public NewProcessRunner(IEnumerable<ApplicationRunOption> applications) {
			if(applications == null)
				throw new ArgumentNullException(nameof(applications));

			Applications = applications.ToList().AsReadOnly();
			if(Applications.Count == 0)
				throw new ArgumentException("Необходимо настроить хотя бы один вариант запуска.", nameof(applications));
			if(Applications.Any(x => x == null))
				throw new ArgumentException("Список вариантов запуска не может содержать null.", nameof(applications));

			SelectedApplication = Applications[0];
		}

		public IReadOnlyList<ApplicationRunOption> Applications { get; }
		public ApplicationRunOption SelectedApplication { get; set; }

		public void Run(LoginToDatabaseResponse loginToDatabase) {
			if(SelectedApplication == null || !Applications.Contains(SelectedApplication))
				throw new InvalidOperationException("Не выбран вариант запуска приложения.");

			string exeFileName = SelectedApplication.ExecutableFileName;
			if(!File.Exists(exeFileName))
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
