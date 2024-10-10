namespace QS.Launcher {
	public class LauncherOptions {
		/// <summary>
		/// Картинка с логотипом
		/// </summary>
		public byte[] LogoImage { get; set; }

		/// <summary>
		/// Иконка приложения
		/// </summary>
		public byte[] LogoIcon { get; set; }

		public string AppTitle { get; set; }

		public string AppExecutablePath { get; set; }
		
		/// <summary>
		/// Путь к файлу json с конфигурацией подключений.
		/// </summary>
		public string ConnectionsJsonFileName { get; set; }
		
		/// <summary>
		/// Путь к старому конфигу приложения, для переноса настроек.
		/// </summary>
		public string? OldConfigFilename { get; set; }
	}
}
