using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QS.DBScripts.Models {
	public abstract class BaseMySqlDbLoader : IDbCreatorModel {
		static protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		protected readonly string connectionString;
		protected readonly IProgressBarDisplayable progress;
		protected readonly IDbCreatorInteraction interaction;
		protected readonly CancellationToken cancellationToken;
		private readonly bool justCreated;
		private readonly bool preserveUsers;

		public bool FillBaseGuid { get; set; } = true;
		protected string lastExecutedStatement = "";

		public BaseMySqlDbLoader(DbCreationResources resources) {
			if(resources == null)
				throw new ArgumentNullException(nameof(resources));
			if(string.IsNullOrWhiteSpace(resources.ConnectionString))
				throw new ArgumentException("Connection string is required", nameof(resources));
			this.connectionString = resources.ConnectionString;
			this.progress = resources.Progress ?? throw new ArgumentNullException(nameof(resources.Progress));
			this.interaction = resources.Interactions ?? throw new ArgumentNullException(nameof(resources.Interactions));
			this.cancellationToken = resources.CancellationToken;
			this.justCreated = resources.JustCreated;
			this.preserveUsers = resources.PreserveUsers;
		}

		protected virtual Version NewBaseVersion => null;

		/// <summary>
		/// Метод блокирует вызывающий поток на время работы с базой
		/// Вынесение в фоновый поток — ответственность вызывающего кода
		/// </summary>
		public bool RunCreation(string dbName, string dbTitle) {
			bool needPreserveUsers = preserveUsers;
			using(var connectionDB = new MySqlConnection(connectionString)) {
				try {
					logger.Info("Connecting to MySQL...");
					connectionDB.Open();
					var cmd = new MySqlCommand(connectionDB, null);

					bool hasBase = false;
					bool needDropBase = false;
					if(!justCreated) {
						logger.Info("Проверяем существует ли уже база.");

						cmd.CommandText = "SHOW DATABASES;";
						using(var rdr = cmd.ExecuteReader()) {
							while(rdr.Read()) {
								if(rdr[0].ToString() == dbName) {
									hasBase = true;
									break;
								}
							}
						}

						if(hasBase) {
							switch(interaction.AskDropExistingDatabase(dbName)) {
								case ToDoWithExistingDatabase.Nothing:
									logger.Info("Пользователь отказался от работы с существующей базой {0}.", dbName);
									return false;
								case ToDoWithExistingDatabase.Recreate:
									needDropBase = true;
									break;
								case ToDoWithExistingDatabase.Rewrite:
									needPreserveUsers = true;
									break;
							}
						}

						if(needDropBase) {
							logger.Info("Удаляем существующую базу {0}.", dbName);
							progress.Add(text: $"Удаляем существующую базу {dbName}");
							cmd.CommandText = String.Format("DROP DATABASE `{0}`", dbName);
							cmd.ExecuteNonQuery();
						}

						if(!hasBase || needDropBase) {
							logger.Info("Создаем новую базу.");
							progress.Add(text: $"Создаем базу {dbName}");
							cmd.CommandText = String.Format("CREATE SCHEMA `{0}` DEFAULT CHARACTER SET utf8mb4 ;", dbName);
							cmd.ExecuteNonQuery();
						}
					}
					cmd.CommandText = String.Format("USE `{0}` ;", dbName);

					cmd.ExecuteNonQuery();

					List<Dictionary<string, object>> savedUsers = null;
					if(needPreserveUsers) {
						savedUsers = ReadUsersTable(cmd);
						// если users нет - сохранять нечего, перезапись эквивалентна пересозданию и версия не важна
						if(savedUsers != null && !VersionMatches(cmd, out string versionError)) {
							interaction.ReportError(versionError, lastExecutedStatement);
							return false;
						}
						DropAllSchemaObjects(cmd);
					}

					ExecutScript(cmd);

					if(savedUsers != null && savedUsers.Count > 0)
						RestoreUsers(cmd, savedUsers);

					if(FillBaseGuid) {
						logger.Info("Генерируем BaseGuid");
						cmd.CommandText =
							"INSERT INTO base_parameters (name, str_value) VALUES ('BaseGuid', @guid) " +
							"ON DUPLICATE KEY UPDATE str_value = VALUES(str_value)";
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@guid", Guid.NewGuid().ToString());
						cmd.ExecuteNonQuery();
						logger.Info("BaseGuid успешно записан.");
					}

					if(dbTitle != null) {
						logger.Info("Генерируем BaseTitle");
						cmd.CommandText =
							"INSERT INTO base_parameters (name, str_value) VALUES ('BaseTitle', @title) " +
							"ON DUPLICATE KEY UPDATE str_value = VALUES(str_value)";
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@title", dbTitle);
						cmd.ExecuteNonQuery();
						logger.Info("BaseTitle успешно записан.");
					}

				}
				catch(OperationCanceledException) {
					logger.Info("Создание базы отменено пользователем.");
					throw;
				}
				catch(InvalidCastException ex) {
					logger.Error(ex, "Ошибка подключения к серверу.");
					interaction.ReportError("Ощибка в работе с MariaDB 10.10", lastExecutedStatement);
					return false;
				}
				catch(MySqlException ex) {
					logger.Error(ex, "Ошибка работы с MySQL.");
					string text;
					if(ex.Number == 1045 || ex.Number == 0)
						text = "Доступ запрещен.\nПроверьте логин и пароль.";
					else if(ex.Number == 1042)
						text = "Не удалось подключиться к серверу БД.";
					else
						text = ex.Message;
					interaction.ReportError(text, lastExecutedStatement);
					return false;
				}
				finally {
					if(progress.IsStarted)
						progress.Close();
				}
			}
			return true;
		}

		protected abstract void ExecutScript(MySqlCommand cmd);

		#region Перезапись с сохранением пользователей

		/// <returns>null - таблицы users в базе нет</returns>
		private List<Dictionary<string, object>> ReadUsersTable(MySqlCommand cmd) {
			cmd.Parameters.Clear();
			cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'users'";
			if(Convert.ToInt32(cmd.ExecuteScalar()) == 0)
				return null;

			logger.Info("Сохраняем пользователей существующей базы.");
			var rows = new List<Dictionary<string, object>>();
			cmd.CommandText = "SELECT * FROM users";
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read()) {
					var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					for(int i = 0; i < rdr.FieldCount; i++)
						row[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
					rows.Add(row);
				}
			}
			return rows;
		}

		/// <summary>
		/// Схема другой версии после перезаписи получила бы номер версии нового наполнения без прогона миграций,
		/// поэтому при несовпадении версий перезапись запрещаем
		/// </summary>
		private bool VersionMatches(MySqlCommand cmd, out string error) {
			error = null;
			var newVersion = NewBaseVersion;
			if(newVersion == null)
				return true;

			string current = null;
			try {
				cmd.Parameters.Clear();
				cmd.CommandText = "SELECT str_value FROM base_parameters WHERE name = 'version'";
				current = cmd.ExecuteScalar() as string;
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "Не удалось прочитать версию существующей базы.");
			}

			if(current == null || !Version.TryParse(current, out var currentVersion)) {
				error = "Не удалось определить версию существующей базы.\nПерезапись с сохранением пользователей невозможна.";
				return false;
			}
			if(currentVersion.Major != newVersion.Major || currentVersion.Minor != newVersion.Minor) {
				error = $"Версия существующей базы ({currentVersion}) не совпадает с версией создаваемой ({newVersion.ToString(2)}).\n" +
					"Сначала обновите существующую базу или пересоздайте её полностью.";
				return false;
			}
			return true;
		}

		private void DropAllSchemaObjects(MySqlCommand cmd) {
			logger.Info("Удаляем все объекты существующей базы.");
			progress.Add(text: "Очищаем существующую базу");
			cmd.Parameters.Clear();

			var tables = new List<string>();
			var views = new List<string>();
			cmd.CommandText = "SELECT table_name, table_type FROM information_schema.tables WHERE table_schema = DATABASE()";
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read()) {
					if("VIEW".Equals(rdr.GetString(1), StringComparison.OrdinalIgnoreCase))
						views.Add(rdr.GetString(0));
					else
						tables.Add(rdr.GetString(0));
				}
			}

			// процедуры и функции таблицам не принадлежат и сами не удалятся,
			// а повторный CREATE из наполнения на существующей упадёт
			var routines = new List<(string Name, string Type)>();
			cmd.CommandText = "SELECT routine_name, routine_type FROM information_schema.routines WHERE routine_schema = DATABASE()";
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read())
					routines.Add((rdr.GetString(0), rdr.GetString(1)));
			}

			cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 0";
			cmd.ExecuteNonQuery();
			if(views.Count > 0) {
				cmd.CommandText = "DROP VIEW IF EXISTS " + string.Join(", ", views.Select(v => $"`{v}`"));
				cmd.ExecuteNonQuery();
			}
			if(tables.Count > 0) {
				cmd.CommandText = "DROP TABLE IF EXISTS " + string.Join(", ", tables.Select(t => $"`{t}`"));
				cmd.ExecuteNonQuery();
			}
			foreach(var routine in routines) {
				cmd.CommandText = $"DROP {routine.Type} IF EXISTS `{routine.Name}`";
				cmd.ExecuteNonQuery();
			}
			cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 1";
			cmd.ExecuteNonQuery();
		}

		private void RestoreUsers(MySqlCommand cmd, List<Dictionary<string, object>> savedUsers) {
			logger.Info("Восстанавливаем {0} пользователей.", savedUsers.Count);
			progress.Add(text: "Восстанавливаем пользователей");
			cmd.Parameters.Clear();

			var newColumns = new List<string>();
			cmd.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = 'users'";
			using(var rdr = cmd.ExecuteReader()) {
				while(rdr.Read())
					newColumns.Add(rdr.GetString(0));
			}
			if(newColumns.Count == 0) {
				logger.Warn("Наполнение не создало таблицу users - восстанавливать пользователей некуда.");
				return;
			}

			// пересечение колонок переживает изменение схемы users между наполнениями;
			// id не переносим - автоинкремент выдаст новые, чтобы не столкнуться с id пользователей из наполнения
			var columns = savedUsers[0].Keys
				.Intersect(newColumns, StringComparer.OrdinalIgnoreCase)
				.Where(c => !string.Equals(c, "id", StringComparison.OrdinalIgnoreCase))
				.ToList();
			if(columns.Count == 0)
				return;
			bool hasLogin = columns.Contains("login", StringComparer.OrdinalIgnoreCase);

			string columnList = string.Join(", ", columns.Select(c => $"`{c}`"));
			string valueList = string.Join(", ", columns.Select((c, i) => $"@p{i}"));

			foreach(var row in savedUsers) {
				cmd.Parameters.Clear();
				for(int i = 0; i < columns.Count; i++)
					cmd.Parameters.AddWithValue($"@p{i}", row.TryGetValue(columns[i], out var value) ? value ?? DBNull.Value : DBNull.Value);

				if(hasLogin) {
					// пользователи с тем же login уже пришли из наполнения (например из дампа) - их не перезаписываем
					cmd.Parameters.AddWithValue("@login", row["login"] ?? DBNull.Value);
					cmd.CommandText = $"INSERT INTO users ({columnList}) " +
						$"SELECT {valueList} FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM users WHERE login = @login)";
				}
				else {
					cmd.CommandText = $"INSERT INTO users ({columnList}) VALUES ({valueList})";
				}
				cmd.ExecuteNonQuery();
			}
			cmd.Parameters.Clear();
		}

		#endregion
	}
}
