using MySqlConnector;
using QS.DBScripts.Controllers;
using System;
using System.Text.RegularExpressions;

namespace QS.DBScripts.Models
{
	public class MySqlDbCreateModel : BaseMySqlDbLoader {
		private readonly CreationScript script;

		public MySqlDbCreateModel(EmbeddedCreationResources resources) : base(resources)
		{
			script = resources.Script;
		}

		public override Version NewBaseVersion => script.Version;

		protected override void ExecutScript(MySqlCommand cmd) {
			progress.Start(text: "Получаем скрипт создания базы");

			string sqlScript = script.GetSqlScript();
			int predictedCount = Regex.Matches(sqlScript, ";").Count;
			logger.Debug("Предполагаем наличие {0} команд в скрипте.", predictedCount);
			progress.Start(maxValue: predictedCount);

			var myscript = new MySqlScript(cmd.Connection, sqlScript);
			myscript.StatementExecuted += Myscript_StatementExecuted;

			progress.Add(text: $"Создаем таблицы");
			var commands = myscript.Execute();
			logger.Debug("Выполнено {0} SQL-команд.", commands);
		}

		private void Myscript_StatementExecuted(object sender, MySqlScriptEventArgs args)
		{
			progress.Add();
			logger.Debug("SQL Command = {0}", args.StatementText);
			lastExecutedStatement = $"[{args.Line}:{args.Position}]{args.StatementText}";
		}
	}
}
