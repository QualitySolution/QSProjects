using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NLog;
using QSSaaS;

namespace QSProjectsLib
{
	public class UserInfo
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private string login;

		private bool loadedInConstructor = false;

		public string Name;
		public int id;
		public bool admin;
		public Dictionary<string, bool> Permissions;

		public string Login {
			get {
				return login;
			}
		}

		[Obsolete("Используйте конструктор из с логином. P.S. В новых версиях экземпляр класса создается автоматически из диалога входа.")]
		public UserInfo ()
		{

		}

		internal UserInfo (string login)
		{
			this.login = login;
			LoadUserInfo ();
		}

		[Obsolete("Устаревший интерфейс, метод вызывать не нужно, проверка происходит в конструкторе.")]
		public bool TestUserExistByLogin(bool CreateNotExist)
		{
			logger.Info("Проверка наличия пользователя в базе...");
			if (loadedInConstructor) {
				logger.Warn ("Информация о пользователе загружена в момент входа, это старый метот, его вызов не требуется.");
				return true;
			}
			try
			{
				QSMain.CheckConnectionAlive();
				string sql = "SELECT COUNT(*) AS cnt FROM users WHERE login = @login";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", login);
				MySqlDataReader rdr = cmd.ExecuteReader();
				rdr.Read();
				bool Exist = false;
				if (rdr["cnt"].ToString() != "0")
					Exist = true;
				rdr.Close();
				
				if( CreateNotExist && !Exist)
				{
					CreateUserRow ();
					Exist = true;
				}
				return Exist;
			}
			catch (Exception ex)
			{
				logger.ErrorException("Ошибка проверки пользователя!", ex);
				return false;
			}
		}

		[Obsolete("Устаревший интерфейс, метод вызывать не нужно! загрузка инфы происходит в конструкторе.")]
		public void UpdateUserInfoByLogin()
		{
			logger.Info("Чтение информации о пользователе...");
			if (loadedInConstructor) {
				logger.Warn ("Информация о пользовтеле загружена в момент входа, вызов функции не требуется при старте программы. Если нужно обновить информацию используйте метод LoadUserInfo");
				return;
			}
			try
			{
				string sql = "SELECT * FROM users WHERE login = @login";
				QSMain.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", login);
				MySqlDataReader rdr = cmd.ExecuteReader();
				rdr.Read();

				Name = rdr["name"].ToString();
				id = rdr.GetInt32("id");
				admin = rdr.GetBoolean (QSMain.AdminFieldName);

				Permissions = new Dictionary<string, bool>();
				foreach( KeyValuePair<string, UserPermission> Right in QSMain.ProjectPermission)
				{
					string FieldName = Right.Value.DataBaseName;
					Permissions.Add (Right.Key, rdr.GetBoolean (FieldName));
				}

				rdr.Close();
			}
			catch (Exception ex)
			{
				logger.ErrorException("Ошибка чтения информации о пользователе!", ex);
			}	
		}

		private void CreateUserRow()
		{
			bool FirstUser = false;
			string sql = "SELECT COUNT(*) AS cnt FROM users";
			var cmd = new MySqlCommand(sql, QSMain.connectionDB);
			using (var rdr = cmd.ExecuteReader ()) {
				rdr.Read ();
				if (rdr ["cnt"].ToString () == "0")
					FirstUser = true;
			}

			logger.Info ("Создаем пользователя в базе");
			sql = "INSERT INTO users (login, name, " + QSMain.AdminFieldName + ") " +
			"VALUES (@login, @login, @admin)";
			cmd = new MySqlCommand (sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue ("@login", login);
			cmd.Parameters.AddWithValue ("@admin", FirstUser);
			cmd.ExecuteNonQuery ();
		}

		public void LoadUserInfo()
		{
			logger.Info("Чтение информации о пользователе...");
			if(login == "root")
			{
				logger.Info ("Вход под root, отмена чтения...");
				return;
			}

			string sql = "SELECT * FROM users WHERE login = @login";
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue("@login", login);
			MySqlDataReader rdr = cmd.ExecuteReader();

			if(!rdr.HasRows)
			{
				rdr.Close ();
				if(Session.IsSaasConnection)
				{
					logger.Info ("Создаем описание пользователя в базе через SAAS...");
					var srv = Session.GetSaaSService ();
					if(!srv.createUserInBase (Session.SessionId))
					{
						throw new ApplicationException ("Сервер SAAS ответил отказом, на попытку добавить пользователя.");
					}
				}
				else
					CreateUserRow ();
				//Перечитываем инфу.
				rdr = cmd.ExecuteReader();
			}
			if(!rdr.HasRows)
				throw new ApplicationException (String.Format ("В БД нет пользователя с логином {0}", Login));

			rdr.Read();

			Name = rdr["name"].ToString();
			id = rdr.GetInt32("id");
			admin = rdr.GetBoolean (QSMain.AdminFieldName);

			Permissions = new Dictionary<string, bool>();
			foreach( KeyValuePair<string, UserPermission> Right in QSMain.ProjectPermission)
			{
				string FieldName = Right.Value.DataBaseName;
				Permissions.Add (Right.Key, rdr.GetBoolean (FieldName));
			}

			rdr.Close();
			loadedInConstructor = true;
		}
			
		public void ChangeUserPassword( Gtk.Window Parrent)
		{
			ChangePassword win = new ChangePassword();
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

		public UserPermission(string DataBaseName, string DisplayName, string Description)
		{
			this.DataBaseName = DataBaseName;
			this.DisplayName = DisplayName;
			this.Description = Description;
		}
	}
}