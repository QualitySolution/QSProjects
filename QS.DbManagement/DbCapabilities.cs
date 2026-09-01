using QS.DBScripts;

namespace QS.DbManagement {
	/// <summary>
	/// комбинирует права пользователя с конфигурацией приложения
	/// </summary>
	public class DbCapabilities {
		private readonly IDbScriptsConfiguration scripts;

		public DbCapabilities(IDbScriptsConfiguration scripts = null) {
			this.scripts = scripts;
		}

		/// <summary>Провайдер отвечает за то, что разрешает сервер</summary>
		public DbCapabilitySet For(IDbProvider provider) {
			if(provider == null)
				return DbCapabilitySet.None;

			return new DbCapabilitySet {
				// создание из встроенного скрипта: и сервер разрешает, и скрипт зарегистрирован
				CanCreate = provider.CanCreateDatabase && scripts?.HasCreationScript() == true,
				// наполнению дампом скрипт не нужен - хватает права на создание
				CanImport = provider.CanCreateDatabase,
				CanDrop = provider.CanDropDatabase,
				CanBackup = provider.CanBackupDatabase,
				CanRefreshMetadata = provider.CanRefreshMetadata,
				CanChangeOwnPassword = provider.CanChangeOwnPassword,
				CanManageUsers = provider.CanManageUsers
			};
		}
	}
}
