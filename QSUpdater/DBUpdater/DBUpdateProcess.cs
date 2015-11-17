using System;
using QSProjectsLib;
using System.IO;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using QSSupportLib;
using Gtk;

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
			progressbarTotal.Text = String.Format ("Обновление: {0} → {1}", 
				StringWorks.VersionToShortString (updateHop.Source),
				StringWorks.VersionToShortString (updateHop.Destanation)
			);
				
			string fileName = System.IO.Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments),
				"Резервные копии",
				String.Format ("{0}{1:yyMMdd-HHmm}.sql", MainSupport.ProjectVerion.Product, DateTime.Now)
			);
			entryFileName.Text = fileName;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			try
			{
				if(checkCreateBackup.Active)
				{
					if(ExecuteBackup ())
						return;
				}

				logger.Info("Обновляем базу данных до версии {0}", StringWorks.VersionToShortString(updateHop.Destanation));
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

				progressbarTotal.Adjustment.Value = progressbarTotal.Adjustment.Upper;
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

		bool ExecuteBackup()
		{
			logger.Info ("Создаем резервную копию базы.");
			if(String.IsNullOrWhiteSpace (entryFileName.Text))
			{
				logger.Warn ("Имя файла резервной копии пустое. Отмена.");
				return true;
			}

			progressbarTotal.Text = "Создание резервной копии";
			progressbarOperation.Visible = true;
			QSMain.WaitRedraw ();

			using (MySqlCommand cmd = QSMain.connectionDB.CreateCommand ())
			{
				using (MySqlBackup mb = new MySqlBackup(cmd))
				{
					var dir = System.IO.Path.GetDirectoryName (entryFileName.Text);

					if (!Directory.Exists (dir))
						Directory.CreateDirectory (dir);

					logger.Debug (entryFileName.Text);
					mb.ExportProgressChanged += Mb_ExportProgressChanged;
					mb.ExportToFile(entryFileName.Text);
				}
			}

			progressbarOperation.Visible = false;
			return false;
		}

		void Mb_ExportProgressChanged (object sender, ExportProgressArgs e)
		{
			progressbarOperation.Text = String.Format ("Экспорт {0}", e.CurrentTableName);
			progressbarOperation.Adjustment.Upper = e.TotalRowsInCurrentTable;
			progressbarOperation.Adjustment.Value = e.CurrentRowIndexInCurrentTable;
			progressbarTotal.Adjustment.Upper = e.TotalTables;
			progressbarTotal.Adjustment.Value = e.CurrentTableIndex;
			QSMain.WaitRedraw ();
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

			progressbarTotal.Text = operationName;
			progressbarTotal.Adjustment.Value = 0;
			progressbarTotal.Adjustment.Upper = predictedCount;
			QSMain.WaitRedraw ();

			var script = new MySqlScript(QSMain.connectionDB, sql);
			script.StatementExecuted += Script_StatementExecuted;
			var commands = script.Execute ();
			logger.Debug ("Выполнено {0} SQL-команд.", commands);
		}

		void Script_StatementExecuted (object sender, MySqlScriptEventArgs args)
		{
			progressbarTotal.Adjustment.Value++;
			textviewLog.Buffer.Text = textviewLog.Buffer.Text + args.StatementText + "\n";
			QSMain.WaitRedraw ();
		}

		protected void OnCheckCreateBackupToggled (object sender, EventArgs e)
		{
			entryFileName.Sensitive = buttonFileChooser.Sensitive = checkCreateBackup.Active;
		}

		protected void OnButtonFileChooserClicked (object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog ("Укажите файл резервной копии",
					this,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.SetFilename (entryFileName.Text);
			fc.Show (); 
			if (fc.Run () == (int)ResponseType.Accept) {
				entryFileName.Text = fc.Filename;
			}
			fc.Destroy ();
		}
	}
}

