using System;

namespace QS.Launcher.AppRunner {
	public class ApplicationRunOption {
		public ApplicationRunOption(string title, string executableFileName) {
			if(string.IsNullOrWhiteSpace(title))
				throw new ArgumentException("Название варианта запуска не может быть пустым.", nameof(title));
			if(string.IsNullOrWhiteSpace(executableFileName))
				throw new ArgumentException("Путь к запускаемому файлу не может быть пустым.", nameof(executableFileName));

			Title = title;
			ExecutableFileName = executableFileName;
		}

		public string Title { get; }
		public string ExecutableFileName { get; }
	}
}
