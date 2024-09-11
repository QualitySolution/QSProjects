using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using NLog;
using QS.Validation;
using QSSaaS;

namespace QSProjectsLib
{
	public class UserInfo
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public int Id;

		public bool Admin;

		string login;

		public string Login { get { return login; } }

		public string Email;
		public string Name;
		public Dictionary<string, bool> Permissions;

		public UserInfo (string login)
		{
			this.login = login;
			LoadUserInfo ();
		}

		private void CreateUserRow ()
		{
			bool FirstUser = false;
			string sql = "SELECT COUNT(*) AS cnt FROM users";
			var cmd = new MySqlCommand (sql, QSMain.connectionDB);
			using (var rdr = cmd.ExecuteReader ()) {
				rdr.Read ();
				if (rdr ["cnt"].ToString () == "0")
					FirstUser = true;
			}

			logger.Info ("Создаем пользователя в базе");
			sql = "INSERT INTO users (login, name, " + QSMain.AdminFieldName + ", deactivated) " +
				"VALUES (@login, @login, @admin, @deactivated)";
			cmd = new MySqlCommand (sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue ("@login", login);
			cmd.Parameters.AddWithValue ("@admin", FirstUser);
			cmd.Parameters.AddWithValue ("@deactivated", false);
			cmd.ExecuteNonQuery ();
		}

		public void LoadUserInfo ()
		{
			logger.Info ("Чтение информации о пользователе...");
			if (login == "root") {
				logger.Info ("Вход под root, отмена чтения...");
				Admin = true;
				Permissions = QSMain.ProjectPermission.ToDictionary(x => x.Key, x => false);
				return;
			}

			string sql = "SELECT * FROM users WHERE login = @login";
			MySqlCommand cmd = new MySqlCommand (sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue ("@login", login);
			MySqlDataReader rdr = cmd.ExecuteReader ();

			if (!rdr.HasRows) {
				rdr.Close ();
				if (Session.IsSaasConnection) {
					logger.Info ("Создаем описание пользователя в базе через SAAS...");
					var srv = Session.GetSaaSService ();
					if (!srv.createUserInBase (Session.SessionId)) {
						throw new ApplicationException ("Сервер SAAS ответил отказом, на попытку добавить пользователя.");
					}
				} else
					CreateUserRow ();
				//Перечитываем инфу.
				rdr = cmd.ExecuteReader ();
			}
			if (!rdr.HasRows)
				throw new ApplicationException (String.Format ("В БД нет пользователя с логином {0}", Login));

			rdr.Read ();

			Name = rdr ["name"].ToString ();
			Id = rdr.GetInt32 ("id");
			Admin = rdr.GetBoolean (QSMain.AdminFieldName);
			var fieldNames = Enumerable.Range(0, rdr.FieldCount).Select(i => rdr.GetName(i)).ToArray();

			Permissions = new Dictionary<string, bool> ();
			foreach (KeyValuePair<string, UserPermission> Right in QSMain.ProjectPermission) {
				string FieldName = Right.Value.DataBaseName;
				if(fieldNames.Contains(FieldName))
					Permissions.Add (Right.Key, rdr.GetBoolean (FieldName));
				else
					Permissions.Add(Right.Key, false);
			}

			rdr.Close ();
			Email = tryGetUserEmail ();
		}

		string tryGetUserEmail ()
		{
			try {
				string sql = "SELECT email FROM users WHERE @id = id;";
				var cmd = new MySqlCommand (sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", Id);
				using (var rdr = cmd.ExecuteReader ()) {
					if (rdr.Read ())
						return DBWorks.GetString (rdr, "email", String.Empty);
				} 
			} catch (MySqlException myEx) {
				if (myEx.Number == 1054) {
					logger.Warn (myEx, "В базе отсутствует поле email в таблице users. Требуется обновление версии базы.");
					return String.Empty;
				} else
					throw myEx;
			}
			return String.Empty;
		}

		public void ChangeUserPassword (Gtk.Window Parrent)
		{
			ChangePassword win = new ChangePassword ();
			//win.ParentWindow = Parrent;
			win.TransientFor = Parrent;
			win.Show ();
			win.Run ();
			win.Destroy ();
		}

		public void ChangeUserPassword (Gtk.Window Parrent, IPasswordValidator passwordValidator)
		{
			ChangePassword win = new ChangePassword (passwordValidator);
			//win.ParentWindow = Parrent;
			win.TransientFor = Parrent;
			win.Show ();
			win.Run ();
			win.Destroy ();
		}
	}

	public class UserPermission
	{
		public string DataBaseName;
		public string DisplayName;
		public string Description;

		public UserPermission (string DataBaseName, string DisplayName, string Description)
		{
			this.DataBaseName = DataBaseName;
			this.DisplayName = DisplayName;
			this.Description = Description;
		}
	}
}
