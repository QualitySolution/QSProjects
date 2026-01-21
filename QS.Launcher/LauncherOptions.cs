using System;
using System.Collections.Generic;

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
		
		/// <summary>
		/// Путь к файлу json с конфигурацией подключений.
		/// </summary>
		public string ConnectionsJsonFileName { get; set; }
		
		/// <summary>
		/// Путь к старому конфигу приложения, для переноса настроек.
		/// </summary>
		public string OldConfigFilename { get; set; }
		
		public Func<List<Dictionary<string, string>>> MakeDefaultConnections { get; set; }
		
		/// <summary>
		/// Указывает, запущен ли лаунчер как отдельное standalone приложение.
		/// True - лаунчер запускается отдельно и запускает другое приложение в новом процессе.
		/// False - лаунчер работает в одном процессе с основным приложением (in-process режим).
		/// В standalone режиме доступна галочка "Не закрывать лаунчер после подключения".
		/// </summary>
		public bool IsStandalone { get; set; } = true;
	}
}
