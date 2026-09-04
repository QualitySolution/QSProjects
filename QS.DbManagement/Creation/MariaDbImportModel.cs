using MySqlConnector;
using QS.DBScripts.Models;

namespace QS.DbManagement.Creation {
	/// <summary>
	/// Наполнение MariaDB базы пользовательским дампом.
	/// Метод блокирует вызывающий поток — выносить в фон ответственность вызывающего кода.
	/// </summary>
	public class MariaDbImportModel : BaseMySqlDbLoader {
		private string DumpFilePath { get; }

		public MariaDbImportModel(
			DbDumpResources resources)
			: base(resources) {
			DumpFilePath = resources.DumpFilePath;
		}

		protected override void ExecutScript(MySqlCommand cmd) {
			cmd.CommandTimeout = 0;
			using(var backup = new MySqlBackup(cmd)) {
				// ускоряет дамп
				backup.Command.CommandText = "SET SESSION foreign_key_checks = 0, unique_checks = 0;";
				backup.Command.ExecuteNonQuery();

				bool started = false;
				backup.ImportProgressChanged += (sender, args) => {
					if(cancellationToken.IsCancellationRequested) {
						((MySqlBackup)sender).StopAllProcess();
						return;
					}
					if(!started) {

						logger.Debug("Предполагаем наличие {0} команд в скрипте.", args.TotalBytes);
						progress?.Start(maxValue: args.TotalBytes, text: "Импорт дампа в базу данных");
						started = true;
					}
					progress?.Update(args.CurrentBytes);
				};
				backup.ImportFromFile(DumpFilePath);
			}
		}
	}
}
