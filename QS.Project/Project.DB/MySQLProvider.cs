using MySqlConnector;
using NLog;
using QS.Dialog;
using QS.Project.Services;
using System;
using System.Threading;

namespace QS.Project.DB {
	public class MySQLProvider : IMySQLProvider
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private bool waitResultIsOk;
		private readonly IRunOperationService runOperationService;
		private readonly IInteractiveQuestion question;
		public MySqlConnection DbConnection { get; private set; }

		public MySQLProvider(MySqlConnectionStringBuilder connectionStringBuilder, IRunOperationService runOperationService, IInteractiveQuestion question)
			: this(connectionStringBuilder.ConnectionString, runOperationService, question)
		{
		}

		public MySQLProvider(string connectionString, IRunOperationService runOperationService, IInteractiveQuestion question)
		{
			this.runOperationService = runOperationService;
			this.question = question;
			DbConnection = new MySqlConnection(connectionString);
			DbConnection.Open();
		}

		public MySQLProvider(IRunOperationService runOperationService, IInteractiveQuestion question)
		{
			this.runOperationService = runOperationService;
			this.question = question;
			DbConnection = (MySqlConnection)Connection.ConnectionDB;
		}

		public void CheckConnectionAlive()
		{		
			logger.Info("Проверяем соединение...");

			bool timeout = runOperationService.GetRunOperationView().RunOperation(
				new ThreadStart(DoPing),
				DbConnection.ConnectionTimeout,
				"Идет проверка соединения с базой данных.");

			
			if(timeout && DbConnection.State == System.Data.ConnectionState.Open) {
				DbConnection.Close(); //На линуксе есть случаи когда состояние соединения не корректное.
			}
			if(DbConnection.State != System.Data.ConnectionState.Open) {
				logger.Warn("Соединение с сервером разорвано, пробуем пересоединится...");
				TryConnect();
			}
			logger.Info("Ок.");
		}

		public void TryConnect()
		{
			logger.Info("Пытаемся восстановить соединение...");

			bool timeout = runOperationService.GetRunOperationView().RunOperation(new ThreadStart(DoConnect),
							   DbConnection.ConnectionTimeout,
							   "Соединяемся с сервером MySQL.");

			if(!waitResultIsOk || timeout) {
				bool result = question.Question("Соединение было разорвано. Повторить попытку подключения? В противном случае приложение завершит работу.", "Соединение разорвано");
				if(result) {
					TryConnect();
				} else {
					Environment.Exit(1);
				}
			}
		}

		private void DoPing()
		{
			DbConnection.Ping();
			logger.Debug("Конец пинга соединения.");
		}

		private void DoConnect()
		{
			waitResultIsOk = true;
			try {
				DbConnection.Open();
			} catch(Exception ex) {
				logger.Warn(ex, "Не удалось соединится.");
				waitResultIsOk = false;
			}
		}
	}
}
