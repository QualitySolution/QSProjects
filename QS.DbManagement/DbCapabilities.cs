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
		public bool CanCreate(IDbManager manager) {
			return manager?.CanCreateDatabase == true
				&& scripts?.HasCreationScript() == true;
		}

		/// <summary>
		/// Наполнение дампом, если
		/// есть права на создание
		/// </summary>
		public bool CanImport(IDbManager manager) {
			return manager?.CanCreateDatabase == true;
		}

		/// <summary>по возможности провайдера</summary>
		public bool CanBackup(IDbManager manager) {
			return manager?.CanBackupDatabase == true;
		}

		/// <summary>по праву провайдера</summary>
		public bool CanDrop(IDbManager manager) {
			return manager?.CanDropDatabase == true;
		}

		public bool CanChangeOwnPassword(IDbUserManager userManager) {
			return userManager?.CanChangeOwnPassword == true;
		}

		public bool CanManageUsers(IDbUserManager userManager) {
			return userManager?.CanManageUsers == true;
		}

		public bool CanRefreshMetadata(IDbManager manager) {
			return manager?.CanRefreshMetadata == true;
		}
	}
}
