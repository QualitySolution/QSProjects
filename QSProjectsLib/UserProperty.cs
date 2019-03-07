using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib.Permissions;
using QSSaaS;

namespace QSProjectsLib
{
	public partial class UserProperty : Gtk.Dialog
	{
		#region Глобальные настройки

		public static Func<List<IPermissionsView>> PermissionViewsCreator;

		#endregion

		private static Logger logger = LogManager.GetCurrentClassLogger();
		private const string passFill = "n0tChanG3d";
		public bool NewUser;
		string OriginLogin;
		Dictionary<string, CheckButton> RightCheckButtons;
		List<IPermissionsView> permissionViews;

		public UserProperty()
		{
			this.Build();
			RightCheckButtons = new Dictionary<string, CheckButton>();
			if(QSMain.ProjectPermission != null) {
				foreach(KeyValuePair<string, UserPermission> Pair in QSMain.ProjectPermission) {
					CheckButton CheckBox = new CheckButton();
					CheckBox.Label = Pair.Value.DisplayName;
					CheckBox.TooltipText = Pair.Value.Description;
					RightCheckButtons.Add(Pair.Key, CheckBox);
					vboxPermissions.PackStart(CheckBox, false, false, 0);
				}
				vboxPermissions.ShowAll();
			}

			if(PermissionViewsCreator != null) {
				permissionViews = PermissionViewsCreator();

				foreach(var view in permissionViews) {
					var tabLabel = new Label(view.ViewName);
					notebook1.AppendPage((Widget)view, tabLabel);
					(view as Widget).Show();
				}
			}
		}

		public void UserFill(int UserId)
		{
			NewUser = false;
			logger.Info("Запрос пользователя №{0}...", UserId);
			string sql = "SELECT * FROM users WHERE users.id = @id";
			try {
				QSMain.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

				cmd.Parameters.AddWithValue("@id", UserId);

				MySqlDataReader rdr = cmd.ExecuteReader();

				rdr.Read();
				bool deactivated = false;
				string email = String.Empty;
				try {
					deactivated = DBWorks.GetBoolean(rdr, "deactivated", false);
					email = DBWorks.GetString(rdr, "email", String.Empty);
				} catch {
				}

				entryID.Text = rdr["id"].ToString();
				entryLogin.Text = rdr["login"].ToString();
				//Если SaaS - запретить редактировать логин.
				entryLogin.Sensitive = !Session.IsSaasConnection;
				OriginLogin = rdr["login"].ToString();
				entryName.Text = rdr["name"].ToString();
				entryPassword.Text = passFill;
				entryEmail.Text = email;
				checkDeactivated.Active = deactivated;
				checkDeactivated.Sensitive = QSMain.User.Login != OriginLogin;
				checkAdmin.Active = rdr.GetBoolean(QSMain.AdminFieldName);

				if(deactivated && Session.IsSaasConnection) { //FIXME Очень странное условие. Нужно разобраться. Что делает этот блок и зачем?
					entryName.Sensitive = entryPassword.Sensitive = entryEmail.Sensitive = false;
					checkDeactivated.Sensitive = checkAdmin.Sensitive = false;
				}

				foreach(KeyValuePair<string, CheckButton> Pair in RightCheckButtons) {
					Pair.Value.Active = rdr.GetBoolean(QSMain.ProjectPermission[Pair.Key].DataBaseName);
				}

				if(permissionViews != null)
				{
					foreach(var view in permissionViews) {
						view.DBFieldValue = DBWorks.GetString(rdr, view.DBFieldName);
					}
				}

				textviewComments.Buffer.Text = rdr["description"].ToString();
				rdr.Close();

				this.Title = entryName.Text;
				logger.Info("Ok");
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка получения информации о пользователе!");
				QSMain.ErrorMessage(this, ex);
			}
		}

		protected void OnButtonOkClicked(object sender, EventArgs e)
		{
			string sql;
			ISaaSService svc = null;

			if(entryLogin.Text == "root") {
				string Message = "Операции с пользователем root запрещены.";
				MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
									   MessageType.Warning,
									   ButtonsType.Ok,
									   Message);
				md.Run();
				md.Destroy();
				return;
			}

			if(Session.IsSaasConnection) {
				int dlgRes;
				svc = Session.GetSaaSService();
				if(NewUser) {
					//Проверка существует ли логин
					sql = "SELECT COUNT(*) FROM users WHERE login = @login";
					QSMain.CheckConnectionAlive();
					MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
					cmd.Parameters.AddWithValue("@login", entryLogin.Text);
					if(Convert.ToInt32(cmd.ExecuteScalar()) > 0) {
						string Message = "Пользователь с логином " + entryLogin.Text + " уже существует в базе. " +
										 "Создание второго пользователя с таким же логином невозможно.";
						MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
											   MessageType.Warning,
											   ButtonsType.Ok,
											   Message);
						md.Run();
						md.Destroy();
						return;
					}
					//Регистрируем пользователя в SaaS
					Result result = svc.registerUserV3(entryLogin.Text, entryPassword.Text, Session.SessionId, entryName.Text, entryEmail.Text);
					if(!result.Success) {
						if(result.Error == ErrorType.UserExists) {
							MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
												   MessageType.Warning,
												   ButtonsType.YesNo,
												   "Пользователь с таким логином уже существует. Предоставить ему доступ к базе?\n" +
												   "ВНИМАНИЕ: Если вы указали новый пароль для пользователя, то он применен не будет.");
							dlgRes = md.Run();
							md.Destroy();
							if((ResponseType)dlgRes == ResponseType.No)
								return;
						} else {
							MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
												   MessageType.Warning,
												   ButtonsType.Close,
												   result.Description);
							md.Run();
							md.Destroy();
							return;
						}
					}
					//Создаем запись в Users.
					sql = "INSERT INTO users (name, login, deactivated, email, " + QSMain.AdminFieldName + ", description" + QSMain.GetPermissionFieldsForSelect() + GetExtraFieldsForSelect() + ") " +
						  "VALUES (@name, @login, @deactivated, @email, @admin, @description" + QSMain.GetPermissionFieldsForInsert() + GetExtraFieldsForInsert() + ")";
				} else {
					if(entryPassword.Text != passFill) {
						MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
											   MessageType.Warning,
											   ButtonsType.YesNo,
											   "Изменение пароля произойдет во всех базах, к которым пользователь имеет доступ.\nПродолжить?");
						dlgRes = md.Run();
						md.Destroy();
						if((ResponseType)dlgRes == ResponseType.Yes && !svc.changeUserPasswordByLogin(entryLogin.Text, Session.Account, entryPassword.Text)) {
							MessageDialog md1 = new MessageDialog(this, DialogFlags.DestroyWithParent,
													MessageType.Warning,
													ButtonsType.Close,
													"Ошибка изменения пароля пользователя.");
							md1.Run();
							md1.Destroy();
							return;
						} else if((ResponseType)dlgRes != ResponseType.Yes)
							return;
					}
					sql = "UPDATE users SET name = @name, deactivated = @deactivated, email = @email, " + QSMain.AdminFieldName + " = @admin," +
					      "description = @description " + QSMain.GetPermissionFieldsForUpdate() + GetExtraFieldsForUpdate() + " WHERE id = @id";
				}
			} else {
				if(NewUser) {
					if(!CreateLogin())
						return;
					sql = "INSERT INTO users (name, login, deactivated, email, " + QSMain.AdminFieldName + ", description" + QSMain.GetPermissionFieldsForSelect()+ GetExtraFieldsForSelect() + ") " +
					      "VALUES (@name, @login, @deactivated, @email, @admin, @description" + QSMain.GetPermissionFieldsForInsert() + GetExtraFieldsForInsert() + ")";
				} else {
					if(OriginLogin != entryLogin.Text && !RenameLogin())
						return;
					if(entryPassword.Text != passFill)
						ChangePassword();
					sql = "UPDATE users SET name = @name, deactivated = @deactivated, email = @email, login = @login, " + QSMain.AdminFieldName + " = @admin," +
					      "description = @description " + QSMain.GetPermissionFieldsForUpdate() + GetExtraFieldsForUpdate() + " WHERE id = @id";
				}
				UpdatePrivileges();
				logger.Info("Запись пользователя...");
			}
			try {
				QSMain.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

				cmd.Parameters.AddWithValue("@id", entryID.Text);
				cmd.Parameters.AddWithValue("@name", entryName.Text);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				cmd.Parameters.AddWithValue("@admin", checkAdmin.Active);
				cmd.Parameters.AddWithValue("@deactivated", checkDeactivated.Active);
				cmd.Parameters.AddWithValue("@email", entryEmail.Text);
				foreach(KeyValuePair<string, CheckButton> Pair in RightCheckButtons) {
					cmd.Parameters.AddWithValue("@" + QSMain.ProjectPermission[Pair.Key].DataBaseName,
						Pair.Value.Active);
				}

				if(permissionViews != null)
				{
					foreach(var view in permissionViews) {
						cmd.Parameters.AddWithValue(view.DBFieldName, view.DBFieldValue);
					}
				}

				if(textviewComments.Buffer.Text == "")
					cmd.Parameters.AddWithValue("@description", DBNull.Value);
				else
					cmd.Parameters.AddWithValue("@description", textviewComments.Buffer.Text);

				cmd.ExecuteNonQuery();
				if(QSMain.User.Login == entryLogin.Text)
					QSMain.User.LoadUserInfo();
				logger.Info("Ok");
				Respond(ResponseType.Ok);
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка записи пользователя!");
				QSMain.ErrorMessage(this, ex);
			}

			//Предоставляем пользователю доступ к базе
			if(Session.IsSaasConnection) {
				if(!svc.changeBaseAccessFromProgram(Session.SessionId, entryLogin.Text, Session.SaasBaseName, !checkDeactivated.Active, checkAdmin.Active)) {
					MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
										  MessageType.Warning,
										  ButtonsType.Close,
										  "Ошибка предоставления доступа к базе данных.");
					md.Run();
					md.Destroy();
					return;
				}
			}
		}

		bool CreateLogin()
		{
			logger.Info("Создание учетной записи на сервере...");

			try {
				//Проверка существует ли логин
				string sql = "SELECT COUNT(*) FROM users WHERE login = @login";
				QSMain.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				if(Convert.ToInt32(cmd.ExecuteScalar()) > 0) {
					string Message = "Пользователь с логином " + entryLogin.Text + " уже существует в базе. " +
									 "Создание второго пользователя с таким же логином невозможно.";
					MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
										   MessageType.Warning,
										   ButtonsType.Ok,
										   Message);
					md.Run();
					md.Destroy();
					return false;
				}

				sql = "SELECT COUNT(*) from mysql.user WHERE USER = @login";
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				try {
					if(Convert.ToInt32(cmd.ExecuteScalar()) > 0) {
						string Message = "Пользователь с логином " + entryLogin.Text + " уже существует на сервере. " +
										 "Если он использовался для доступа к другим базам, может возникнуть путаница. " +
										 "Использовать этот логин?";
						MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
											   MessageType.Warning,
											   ButtonsType.YesNo,
											   Message);
						bool result = (ResponseType)md.Run() == ResponseType.Yes;
						md.Destroy();
						return result;
					}
				} catch(MySqlException ex) {
					if(ex.Number == 1045)
						logger.Warn(ex, "Нет доступа к таблице пользователей, пробую создать пользователя в слепую.");
					else
						return false;
				}
				//Создание пользователя.
				sql = "CREATE USER @login IDENTIFIED BY @password";
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				cmd.Parameters.AddWithValue("@password", entryPassword.Text);
				cmd.ExecuteNonQuery();
				cmd.CommandText = "CREATE USER @login @'localhost' IDENTIFIED BY @password";
				cmd.ExecuteNonQuery();

				logger.Info("Ok");
				return true;
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка создания пользователя!");
				QSMain.ErrorMessage(this, ex);
				return false;
			}

		}

		bool RenameLogin()
		{
			logger.Info("Переименование учетной записи на сервере...");
			try {
				QSMain.CheckConnectionAlive();
				//Проверка существует ли логин
				string sql = "SELECT COUNT(*) from mysql.user WHERE USER = @login";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				try {
					if(Convert.ToInt32(cmd.ExecuteScalar()) > 0) {
						string Message = "Пользователь с логином " + entryLogin.Text + " уже существует на сервере. " +
										 "Переименование невозможно.";
						MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
											   MessageType.Error,
											   ButtonsType.Ok,
											   Message);
						md.Destroy();
						return false;
					}
				} catch(MySqlException ex) {
					if(ex.Number == 1045)
						logger.Warn(ex, "Нет доступа к таблице пользователей, пробую создать пользователя в слепую.");
					else
						return false;
				}

				//Переименование пользователя.
				sql = "RENAME USER @old_login TO @new_login, @old_login @'localhost' TO @new_login @'localhost'";
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@old_login", OriginLogin);
				cmd.Parameters.AddWithValue("@new_login", entryLogin.Text);
				cmd.ExecuteNonQuery();
				logger.Info("Ok");
				return true;
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка переименования пользователя!");
				QSMain.ErrorMessage(this, ex);
				return false;
			}
		}

		void ChangePassword()
		{
			logger.Info("Отправляем новый пароль на сервер...");
			if(Session.IsSaasConnection) {
				ISaaSService svc = Session.GetSaaSService();
				if(!svc.changeUserPasswordByLogin(entryLogin.Text, Session.Account, entryPassword.Text))
					logger.Error("Ошибка установки пароля!");
				else
					logger.Info("Пароль изменен. Ok");
			} else {
				string sql;
				try {
					QSMain.CheckConnectionAlive();
					sql = "SET PASSWORD FOR @login = PASSWORD(@password)";
					MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
					cmd.Parameters.AddWithValue("@login", entryLogin.Text);
					cmd.Parameters.AddWithValue("@password", entryPassword.Text);
					cmd.ExecuteNonQuery();
					cmd.CommandText = "SET PASSWORD FOR @login @'localhost' = PASSWORD(@password)";
					cmd.ExecuteNonQuery();
					logger.Info("Пароль изменен. Ok");
				} catch(Exception ex) {
					logger.Error(ex, "Ошибка установки пароля!");
					QSMain.ErrorMessage(this, ex);
				}
			}
		}

		void UpdatePrivileges()
		{
			logger.Info("Устанавливаем права...");
			try {
				string privileges;
				if(checkAdmin.Active)
					privileges = "ALL";
				else
					privileges = "SELECT, INSERT, UPDATE, DELETE, EXECUTE, SHOW VIEW";
				string sql = $"GRANT {privileges} ON `{QSMain.connectionDB.Database}`.* TO @login, @login @'localhost'";
				QSMain.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				cmd.ExecuteNonQuery();
				if(checkAdmin.Active) {
					cmd.CommandText = "GRANT CREATE USER, GRANT OPTION ON *.* TO @login, @login @'localhost'";
				} else {
					cmd.CommandText = "REVOKE CREATE USER, GRANT OPTION ON *.* FROM @login, @login @'localhost'";
				}
				cmd.ExecuteNonQuery();
				bool GrantMake = false;
				if(checkAdmin.Active) {
					sql = "GRANT SELECT, UPDATE ON mysql.* TO @login, @login @'localhost'";
					GrantMake = true;
				} else {
					sql = "SHOW GRANTS FOR @login";
					cmd = new MySqlCommand(sql, QSMain.connectionDB);
					cmd.Parameters.AddWithValue("@login", entryLogin.Text);
					using(MySqlDataReader rdr = cmd.ExecuteReader()) {
						while(rdr.Read()) {
							if(rdr[0].ToString().IndexOf("mysql") > 0)
								GrantMake = true;
						}
					}
					sql = "REVOKE SELECT, UPDATE ON mysql.* FROM @login, @login @'localhost'";
				}
				if(GrantMake) {
					cmd = new MySqlCommand(sql, QSMain.connectionDB);
					cmd.Parameters.AddWithValue("@login", entryLogin.Text);
					cmd.ExecuteNonQuery();
				}
				logger.Info("Права установлены. Ok");
			} catch(MySqlException ex) when(ex.Number == 1044) {
				logger.Error(ex, "Ошибка установки прав!");
				MessageDialogWorks.RunErrorDialog("У вас не достаточно прав на сервере MySQL для установки полномочий другим пользователям. Возможно некоторые права не были установлены.");
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка установки прав!");
				QSMain.ErrorMessage(this, ex);
			}
		}

		#region Дополнительные вкладки

		public string GetExtraFieldsForUpdate()
		{
			if(permissionViews == null || permissionViews.Count == 0)
				return String.Empty;
			
			string FieldsString = "";
			foreach(var view in permissionViews) {
				FieldsString += ", " + view.DBFieldName + " = @" + view.DBFieldName;
			}
			return FieldsString;
		}

		public string GetExtraFieldsForSelect()
		{
			if(permissionViews == null || permissionViews.Count == 0)
				return String.Empty;
			
			return String.Join("", permissionViews.Select(x => $", {x.DBFieldName}"));
		}

		public string GetExtraFieldsForInsert()
		{
			if(permissionViews == null || permissionViews.Count == 0)
				return String.Empty;
			
			return String.Join("", permissionViews.Select(x => $", @{x.DBFieldName}"));
		}

  		#endregion
	}
}