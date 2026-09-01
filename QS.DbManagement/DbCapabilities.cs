using QS.DbManagement.Creation;
using QS.DBScripts;
using QS.DBScripts.Models;

namespace QS.DbManagement {
	/// <summary>
	/// комбинирует права пользователя с конфигурацией приложения
	/// </summary>
	public class DbCapabilities {
		private readonly IDbScriptsConfiguration scripts;
		private readonly DbResourcesCreationMap creationMap;
		public DbCapabilities(IDbScriptsConfiguration scripts = null, DbResourcesCreationMap creationMap = null) {
			this.scripts = scripts;
			this.creationMap = creationMap;
		}

		/// <summary>Провайдер отвечает за то, что разрешает сервер</summary>
		public DbCapabilitySet For(IDbProvider provider) {
			if(provider == null)
				return DbCapabilitySet.None;

			return new DbCapabilitySet {
				CanCreate = provider.CanCreateDatabase
					&& scripts?.HasCreationScript() == true
					&& creationMap != null
					&& creationMap.Contains(typeof(EmbeddedCreationResources)),
				CanImport = provider.CanCreateDatabase
					&& creationMap != null
					&& creationMap.Contains(typeof(DbDumpResources)),
				CanDrop = provider.CanDropDatabase,
				CanBackup = provider.CanBackupDatabase,
				CanRefreshMetadata = provider.CanRefreshMetadata,
				CanChangeOwnPassword = provider.CanChangeOwnPassword,
				CanManageUsers = provider.CanManageUsers
			};
		}
	}
}
