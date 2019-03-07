using System;
using System.Threading;
using MySql.Data.MySqlClient;
using NLog;
using QS.Tools;
using QS.Project.Dialogs;

namespace QS.Project.DB
{
	public class MySQLProvider : IMySQLProvider
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private bool waitResultIsOk;
		public MySqlConnection DbConnection { get; private set; }

		public MySQLProvider(string connectionString)
		{
			CheckNotificationTool();
			DbConnection = new MySqlConnection(Connection.ConnectionString);
			DbConnection.Open();
		}

		public MySQLProvider()
		{
			CheckNotificationTool();
			DbConnection = (MySqlConnection)Connection.ConnectionDB;
		}

		private void CheckNotificationTool()
		{
			if(StaticTools.NotificationViewAgent == null) {
				throw new ApplicationException("Не настроен инструмент уведомления");
			}
		}

		public void CheckConnectionAlive()
		{		
			logger.Info("Проверяем соединение...");

			bool timeout = StaticTools.NotificationViewAgent.GetRunOperationView().RunOperation(
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

			bool timeout = StaticTools.NotificationViewAgent.GetRunOperationView().RunOperation(new ThreadStart(DoConnect),
							   DbConnection.ConnectionTimeout,
							   "Соединяемся с сервером MySQL.");

			if(!waitResultIsOk || timeout) {
				bool result = StaticTools.NotificationViewAgent.GetSimpleUserQuestionView().RunQuestionView("Соединение было разорвано. Повторить попытку подключения? В противном случае приложение завершит работу.", "Соединение разорвано");
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
