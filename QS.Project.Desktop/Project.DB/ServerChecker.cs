using System;
using MySqlConnector;
using QS.Dialog;

namespace QS.Project.DB {
	public static class ServerChecker {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		/// <summary>
		/// Проверка локали сервера.
		/// </summary>
		/// <param name="parent">Если Parent = null, сообщение будет выводиться в nlog. В противном случае в диалоговое окно.</param>
		public static void Check (MySqlConnection connection, IInteractiveMessage interactive)
		{
			string sql = "SHOW VARIABLES LIKE \"character_set_%\";";
			MySqlCommand cmd = new MySqlCommand (sql, connection);
			string message = "";
			using (MySqlDataReader rdr = cmd.ExecuteReader ()) {
				while (rdr.Read ()) {
					logger.Debug (String.Format ("{0} = {1}", rdr ["Variable_name"], rdr ["Value"]));
					switch (rdr ["Variable_name"].ToString ()) {
						case "character_set_server":
							if (rdr ["Value"].ToString () != "utf8" && rdr["Value"].ToString() != "utf8mb3" && rdr["Value"].ToString() != "utf8mb4") {
								message += String.Format ("* character_set_server = {0} - для нормальной работы программы кодировка сервера " +
								                          "должна быть utf8, utf8mb3 или utf8mb4, иначе возможны проблемы с языковыми символами, этот параметр изменяется " +
								                          "в настройках сервера MySQL\\MariaDB.\n", rdr ["Value"].ToString ());
							}
							break;
						case "character_set_database":
							if (rdr ["Value"].ToString () != "utf8" && rdr["Value"].ToString() != "utf8mb3" && rdr["Value"].ToString() != "utf8mb4") {
								message += String.Format ("* character_set_database = {0} - для нормальной работы программы кодировка базы данных " +
								                          "должна быть utf8, utf8mb3 или utf8mb4, иначе возможны проблемы с языковыми символами, измените кодировку для используемой базы.\n", rdr ["Value"].ToString ());
							}
							break;
					}
				}
			}
			if (message != "") {
				logger.Warn(message);
				interactive.ShowMessage(ImportanceLevel.Warning, message);
			}
		}
	}
}
