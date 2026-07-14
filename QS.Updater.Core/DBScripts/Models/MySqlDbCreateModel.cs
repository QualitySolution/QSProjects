using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace QS.DBScripts.Models
{
	public class MySqlDbCreateModel : BaseMySqlDbLoader {
		private readonly CreationScript script;

		public bool FillBaseGuid { get; set; } = true;
		protected override Action<MySqlCommand> ExecutScript { get; set; }

		public MySqlDbCreateModel(MySqlCreationResources resources) : base(resources)
		{
			ExecutScript = GetExecuter();
		}

		public MySqlDbCreateModel(
			string server, uint port, string login, string password,
			CreationScript script,
			IProgressBarDisplayable progress,
			IDbCreatorInteraction interaction,
			CancellationToken cancellationToken)
			: base(
				  new MySqlCreationResources {
					  Progress = progress,
					  Interactions = interaction,
					  CancellationToken = cancellationToken,
					  ConnectionString = new MySqlConnectionStringBuilder {
						  Server = server,
						  Port = port,
						  UserID = login,
						  Password = password,
						  AllowUserVariables = true
					  }.ConnectionString
				  }
			)
		{
			ExecutScript = GetExecuter();
		}

		Action<MySqlCommand> GetExecuter() {
			return (MySqlCommand cmd) => {
				progress.Start(text: "Получаем скрипт создания базы");

				string sqlScript = script.GetSqlScript();
				int predictedCount = Regex.Matches(sqlScript, ";").Count;
				logger.Debug("Предполагаем наличие {0} команд в скрипте.", predictedCount);
				progress.Start(maxValue: predictedCount);
				cmd.CommandText = String.Format("USE `{0}` ;", dbName);

				var myscript = new MySqlScript(cmd.Connection, sqlScript);
				myscript.StatementExecuted += Myscript_StatementExecuted;

				progress.Add(text: $"Создаем таблицы");
				var commands = myscript.Execute();
				logger.Debug("Выполнено {0} SQL-команд.", commands);
			};
		}

		private void Myscript_StatementExecuted(object sender, MySqlScriptEventArgs args)
		{
			progress.Add();
			logger.Debug("SQL Command = {0}", args.StatementText);
			lastExecutedStatement = $"[{args.Line}:{args.Position}]{args.StatementText}";
		}
	}
}
