using System;
using QSProjectsLib;
using System.IO;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using QSSupportLib;

namespace QSUpdater.DB
{
	public partial class DBUpdateProcess : Gtk.Dialog
	{
		static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public bool Success { get; private set;}

		UpdateHop updateHop;

		public DBUpdateProcess (UpdateHop hop)
		{
			this.Build ();

			updateHop = hop;
			progressbar1.Text = String.Format ("Обновление: {0} → {1}", 
				StringWorks.VersionToShortString (updateHop.Source),
				StringWorks.VersionToShortString (updateHop.Destanation)
			);
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			//FIXME Подумать про резервную копию.

			logger.Info("Обновяем базу данных до версии {0}", StringWorks.VersionToShortString(updateHop.Destanation));
			try
			{
				logger.Info ("Проверяем все ли микро обновления установленны.");
				ExecuteMicroUpdates ();

				logger.Info ("Устанавливаем основное обновление.");
				RunOneUpdate (updateHop, String.Format ("Обновление базы данных до версии {0}", 
					StringWorks.VersionToShortString (updateHop.Destanation)
				));

				if(MainSupport.BaseParameters.All.ContainsKey("micro_updates"))
					MainSupport.BaseParameters.RemoveParameter (QSMain.ConnectionDB,
						"micro_updates");

				MainSupport.LoadBaseParameters ();

				logger.Info ("Доустанавливаем микро обновления.");
				ExecuteMicroUpdates ();

				progressbar1.Adjustment.Value = progressbar1.Adjustment.Upper;
				logger.Info("Обновление до версии {0}, завершено.", StringWorks.VersionToShortString(updateHop.Destanation));
				Success = true;
				Respond(Gtk.ResponseType.Ok);
			}
			catch(Exception ex)
			{
				QSMain.ErrorMessageWithLog("Ошибка при обновлении базы..", logger, ex);
				buttonOk.Sensitive = false;
				return;
			}
		}

		void ExecuteMicroUpdates()
		{
			Version currentDB;
			if(MainSupport.BaseParameters.All.ContainsKey("micro_updates"))
				currentDB  = Version.Parse(MainSupport.BaseParameters.All["micro_updates"]);
			else 
				currentDB = Version.Parse(MainSupport.BaseParameters.Version);

			while(DBUpdater.microUpdates.Exists(u => u.Source == currentDB))
			{
				var update = DBUpdater.microUpdates.Find(u => u.Source == currentDB);
				RunOneUpdate (update, String.Format ("Устанавливаем микро-обновление {0}", StringWorks.VersionToShortString (update.Destanation)));
				currentDB = update.Destanation;
				MainSupport.BaseParameters.UpdateParameter(
					QSMain.ConnectionDB,
					"micro_updates",
					StringWorks.VersionToShortString(currentDB)
				);
			}
		}

		void RunOneUpdate(UpdateHop updateScript, string operationName)
		{
			logger.Info (operationName);
			textviewLog.Buffer.Text = textviewLog.Buffer.Text + operationName + "\n";

			string sql;
			using(Stream stream = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(updateScript.Resource))
			{
				if(stream == null)
					throw new InvalidOperationException( String.Format("Ресурс {0} указанный в обновлениях не найден.", updateScript.Resource));
				StreamReader reader = new StreamReader(stream);
				sql = reader.ReadToEnd();
			}

			int predictedCount = Regex.Matches( sql, ";").Count;

			logger.Debug ("Предполагаем наличие {0} команд в скрипте.", predictedCount);

			progressbar1.Text = operationName;
			progressbar1.Adjustment.Value = 0;
			progressbar1.Adjustment.Upper = predictedCount;
			QSMain.WaitRedraw ();

			var script = new MySqlScript(QSMain.connectionDB, sql);
			script.StatementExecuted += Script_StatementExecuted;
			var commands = script.Execute ();
			logger.Debug ("Выполнено {0} SQL-команд.", commands);
		}

		void Script_StatementExecuted (object sender, MySqlScriptEventArgs args)
		{
			progressbar1.Adjustment.Value++;
			textviewLog.Buffer.Text = textviewLog.Buffer.Text + args.StatementText + "\n";
			QSMain.WaitRedraw ();
		}
	}
}

