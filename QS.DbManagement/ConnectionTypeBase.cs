using Microsoft.Extensions.DependencyInjection;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Collections.Generic;

namespace QS.DbManagement {

	public abstract class ConnectionTypeBase {
		public string Title { get; protected set; }
		public string ConnectionTypeName { get; protected set; }

		public List<ConnectionParameter> Parameters { get; } = new List<ConnectionParameter>();

		public byte[] IconBytes { get; protected set; }

		public abstract bool CanConnect(IEnumerable<ConnectionParameterValue> parameters);

		public abstract IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null);

		public Func<CreatorFactoryArgs, IDbCreatorModel> CreatorFactory { get; set; }

		public Func<CreatorFactoryArgs, IDbCreatorModel> ImportFactory { get; set; }

		/// <summary>
		/// умеет создавать базу, если
		/// задана фабрика,
		/// в конфигурации есть скрипт создания.
		/// НЕ учитывает права пользователя
		/// </summary>
		public virtual bool SupportsDatabaseCreation(IServiceProvider services) {
			return CreatorFactory != null
				&& services.GetService<IDbScriptsConfiguration>()?.HasCreationScript() == true;
		}

		/// <summary>
		/// умеет наполнять базу дампом, если задана фабрика
		/// 
		/// НЕ учитывает права пользователя
		/// </summary>
		public virtual bool SupportsDatabaseImport(IServiceProvider services) {
			return ImportFactory != null;
		}

		#region Права пользователя по управлению базой

		public bool CanCreateDatabase(IDbProvider provider, IServiceProvider services) {
			return provider != null
				&& provider.CanCreateDatabase
				&& SupportsDatabaseCreation(services);
		}

		public bool CanImportDatabase(IDbProvider provider, IServiceProvider services) {
			return provider != null
				&& provider.CanCreateDatabase
				&& SupportsDatabaseImport(services);
		}

		public bool CanBackupDatabase(IDbProvider provider) {
			return provider != null
				&& provider.CanDropDatabase;
		}

		public bool CanDropDatabase(IDbProvider provider) {
			return provider != null
				&& provider.CanDropDatabase;
		}
		#endregion

		public IDbCreatorModel CreateCreator(CreatorFactoryArgs args) {
			if(CreatorFactory == null)
				throw new InvalidOperationException(
					$"Для типа подключения '{ConnectionTypeName}' не настроена фабрика создания БД");
			return CreatorFactory(args);
		}

		public IDbCreatorModel CreateImporter(CreatorFactoryArgs args) {
			if(ImportFactory == null)
				throw new InvalidOperationException(
					$"Для типа подключения '{ConnectionTypeName}' не настроена фабрика импорта дампа");
			return ImportFactory(args);
		}
	}

	/// <summary>
	///   interaction — канал диалогов с пользователем
	///   serviceProvider — для получения дополнительных зависимостей
	/// </summary>
	public class CreatorFactoryArgs {
		public IDbProvider Provider { get; set; }
		public IProgressBarDisplayable Progress { get; set; }
		public IDbCreatorInteraction Interaction { get; set; }
		public System.Threading.CancellationToken CancellationToken { get; set; }
		public IServiceProvider ServiceProvider { get; set; }

		public string ImportDumpFilePath { get; set; }
	}
}
