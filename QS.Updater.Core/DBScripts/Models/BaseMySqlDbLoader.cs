using MySqlConnector;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Threading;

namespace QS.DBScripts.Models {
	public abstract class BaseMySqlDbLoader : IDbCreatorModel {
		static protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		protected readonly string connectionString;
		protected readonly IProgressBarDisplayable progress;
		protected readonly IDbCreatorInteraction interaction;
		protected readonly CancellationToken cancellationToken;
		private readonly bool justCreated;

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
		}

		/// <summary>
		/// Метод блокирует вызывающий поток на время работы с базой
		/// Вынесение в фоновый поток — ответственность вызывающего кода
		/// </summary>
		public bool RunCreation(string dbName, string dbTitle) {
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
								default:
									// в этом пути перезапись и пересоздание совпадают: база сносится и создаётся заново
									needDropBase = true;
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

					ExecutScript(cmd);

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
	}
}
