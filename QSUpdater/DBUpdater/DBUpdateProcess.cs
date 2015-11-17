using System;
using QSProjectsLib;
using System.IO;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

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
				string sql;

				using(Stream stream = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(updateHop.Resource))
				{
					if(stream == null)
						throw new InvalidOperationException( String.Format("Ресурс {0} указанный в обновлениях не найден.", updateHop.Resource));
					StreamReader reader = new StreamReader(stream);
					sql = reader.ReadToEnd();
				}
				progressbar1.Text = String.Format ("Обновление базы данных до версии {0}", 
					StringWorks.VersionToShortString (updateHop.Destanation)
				);
				int predictedCount = Regex.Matches( sql, ";").Count;

				logger.Debug ("Предпологаем наличие {0} команд.", predictedCount);

				progressbar1.Adjustment.Value = 0;
				progressbar1.Adjustment.Upper = predictedCount;
				QSMain.WaitRedraw ();

				var script = new MySqlScript(QSMain.connectionDB, sql);
				script.StatementExecuted += Script_StatementExecuted;
				var commands = script.Execute ();
				logger.Debug ("Выполнено {0} команд.", commands);

				progressbar1.Adjustment.Value = progressbar1.Adjustment.Upper;
				logger.Info("Обновление до версии {0}, завершено.", StringWorks.VersionToShortString(updateHop.Destanation));
				Success = true;
				Respond(Gtk.ResponseType.Ok);
			}
			catch(Exception ex)
			{
				QSMain.ErrorMessageWithLog("Ошибка при обновлении базы..", logger, ex);
				return;
			}
		}

		void Script_StatementExecuted (object sender, MySqlScriptEventArgs args)
		{
			progressbar1.Adjustment.Value++;
			textviewLog.Buffer.Text = textviewLog.Buffer.Text + args.StatementText + "\n";
			QSMain.WaitRedraw ();
		}
	}
}

