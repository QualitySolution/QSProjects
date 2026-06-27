using QS.DBScripts;

namespace QS.DbManagement {
	/// <summary>
	/// комбинирует права пользователя с конфигурацией приложения
	/// </summary>
	public class DbCapabilities {
		private readonly IDbScriptsConfiguration scripts;

		public DbCapabilities(IDbScriptsConfiguration scripts) {
			this.scripts = scripts;
		}

		/// <summary>
		/// Создание из встроенного скрипта, если
		/// сервер разрешает и зарегистрирован скрипт создания
		/// </summary>
		public bool CanCreate(IDbProvider provider) {
			return provider?.CanCreateDatabase == true
				&& scripts?.HasCreationScript() == true;
		}

		/// <summary>
		/// Наполнение дампом, если
		/// есть права на создание
		/// </summary>
		public bool CanImport(IDbProvider provider) {
			return provider?.CanCreateDatabase == true;
		}

		/// <summary>для любой подключённой базы</summary>
		public bool CanBackup(IDbProvider provider) {
			return provider != null;
		}

		/// <summary>по праву провайдера</summary>
		public bool CanDrop(IDbProvider provider) {
			return provider?.CanDropDatabase == true;
		}
	}
}
