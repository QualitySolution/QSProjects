using System;
using Gtk;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace QSProjectsLib
{
	public partial class UserProperty : Gtk.Dialog
	{
		public bool NewUser;
		string OriginLogin;
		Dictionary<string, Gtk.CheckButton> RightCheckButtons;
		
		public UserProperty ()
		{
			this.Build ();
			RightCheckButtons = new Dictionary<string, CheckButton>();
			if(QSProjectsLib.QSMain.ProjectPermission != null)
			{
				foreach( KeyValuePair<string, QSProjectsLib.UserPermission> Pair in QSProjectsLib.QSMain.ProjectPermission)
				{
					Gtk.CheckButton CheckBox = new CheckButton();
					CheckBox.Label = Pair.Value.DisplayName;
					CheckBox.TooltipText = Pair.Value.Description;
					RightCheckButtons.Add (Pair.Key, CheckBox);
					vboxPermissions.PackStart (CheckBox, false, false, 0);
				}
				vboxPermissions.ShowAll ();
			}
		}
		
		public void UserFill(int UserId)
		{
			NewUser = false;
			
			QSMain.OnNewStatusText(String.Format ("Запрос пользователя №{0}...", UserId));
			string sql = "SELECT * FROM users " +
				"WHERE users.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", UserId);
				
				MySqlDataReader rdr = cmd.ExecuteReader();
				
				rdr.Read();
				
				entryID.Text = rdr["id"].ToString();
				entryLogin.Text = rdr["login"].ToString();
				OriginLogin = rdr["login"].ToString();
				entryName.Text = rdr["name"].ToString();
				entryPassword.Text = "nochanged";
				
				checkAdmin.Active = rdr.GetBoolean (QSProjectsLib.QSMain.AdminFieldName);

				foreach(KeyValuePair<string, Gtk.CheckButton> Pair in RightCheckButtons)
				{
					Pair.Value.Active = rdr.GetBoolean (QSProjectsLib.QSMain.ProjectPermission[Pair.Key].DataBaseName);
				}
				
				textviewComments.Buffer.Text = rdr["description"].ToString();
				rdr.Close();
				
				this.Title = entryName.Text;
				QSMain.OnNewStatusText("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка получения информации о пользователе!");
				QSMain.ErrorMessage(this,ex);
			}
		}
		
		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			
			if(entryLogin.Text == "root")
			{
				string Message = "Операции с пользователем root запрещены.";
				MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
				                                      MessageType.Warning, 
				                                      ButtonsType.Ok,
				                                      Message);
				md.Run ();
				md.Destroy();
				return;
			}
			
			if(NewUser)
			{
				if(!CreateLogin ())
					return;
				sql = "INSERT INTO users (name, login, " + QSMain.AdminFieldName +", description" + QSMain.GetPermissionFieldsForSelect () +") " +
					"VALUES (@name, @login, @admin, @description" + QSMain.GetPermissionFieldsForInsert () + ")";
			}
			else
			{
				if(OriginLogin != entryLogin.Text)
					if(!RenameLogin ())
						return;
				if(entryPassword.Text != "nochanged")
					ChangePassword ();
				sql = "UPDATE users SET name = @name, login = @login, " + QSMain.AdminFieldName +" = @admin," +
					"description = @description " +
					QSMain.GetPermissionFieldsForUpdate () +
					" WHERE id = @id";
			}
			UpdatePrivileges ();
			QSMain.OnNewStatusText("Запись пользователя...");
			try 
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", entryID.Text);
				cmd.Parameters.AddWithValue("@name", entryName.Text);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				cmd.Parameters.AddWithValue("@admin", checkAdmin.Active);
				foreach(KeyValuePair<string, Gtk.CheckButton> Pair in RightCheckButtons)
				{
					cmd.Parameters.AddWithValue("@" + QSMain.ProjectPermission[Pair.Key].DataBaseName, 
					                            Pair.Value.Active);
				}

				if(textviewComments.Buffer.Text == "")
					cmd.Parameters.AddWithValue("@description", DBNull.Value);
				else
					cmd.Parameters.AddWithValue("@description", textviewComments.Buffer.Text);
				
				cmd.ExecuteNonQuery();
				if(QSMain.User.Login == entryLogin.Text)
					QSMain.User.UpdateUserInfoByLogin ();
				QSMain.OnNewStatusText("Ok");
				Respond (ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка записи пользователя!");
				QSMain.ErrorMessage(this,ex);
			}
		}
		
		bool CreateLogin()
		{
			QSMain.OnNewStatusText("Создание учетной записи на сервере...");
			try 
			{
				//Проверка существует ли логин
				string sql = "SELECT COUNT(*) FROM users WHERE login = @login";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				if(Convert.ToInt32(cmd.ExecuteScalar()) > 0)
				{
					string Message = "Пользователь с логином " + entryLogin.Text + " уже существует в базе. " +
						"Создание второго пользователя с таким же логином невозможно.";
					MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
					                                      MessageType.Warning, 
					                                      ButtonsType.Ok,
					                                      Message);
					md.Run ();
					md.Destroy();
					return false;
				}
				
				sql = "SELECT COUNT(*) from mysql.user WHERE USER = @login";
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				try
				{
					if(Convert.ToInt32(cmd.ExecuteScalar()) > 0)
					{
						string Message = "Пользователь с логином " + entryLogin.Text + " уже существует на сервере. " +
							"Если он использовался для доступа к другим базам, может возникнуть путаница. " +
								"Использовать этот логин?";
						MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
						                                      MessageType.Warning, 
						                                      ButtonsType.YesNo,
						                                      Message);
						bool result = (ResponseType)md.Run () == ResponseType.Yes;
						md.Destroy();
						return result;
					}
				}
				catch (MySqlException ex)
				{
					if(ex.Number == 1045)
						QSMain.OnNewStatusText ("Нет доступа к таблице пользователей, пробую создать пользователя в слепую.");
					else
						return false;
				}
				//Создание пользователя.
				sql = "CREATE USER @login IDENTIFIED BY @password";
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				cmd.Parameters.AddWithValue("@password", entryPassword.Text);
				cmd.ExecuteNonQuery();
				sql = "CREATE USER " + entryLogin.Text + "@localhost IDENTIFIED BY @password";
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@password", entryPassword.Text);
				cmd.ExecuteNonQuery();
				
				QSMain.OnNewStatusText("Ok");
				return true;
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка создания пользователя!");
				QSMain.ErrorMessage(this,ex);
				return false;
			}
		}
		
		bool RenameLogin()
		{
			QSMain.OnNewStatusText("Переименование учетной записи на сервере...");
			try 
			{
				//Проверка существует ли логин
				string sql = "SELECT COUNT(*) from mysql.user WHERE USER = @login";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				try
				{
					if( Convert.ToInt32(cmd.ExecuteScalar()) > 0)
					{
						string Message = "Пользователь с логином " + entryLogin.Text + " уже существует на сервере. " +
							"Переименование невозможно.";
						MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
						                                      MessageType.Error, 
						                                      ButtonsType.Ok,
						                                      Message);
						md.Destroy();
						return false;
					}
				}
				catch (MySqlException ex)
				{
					if(ex.Number == 1045)
						QSMain.OnNewStatusText ("Нет доступа к таблице пользователей, пробую создать пользователя в слепую.");
					else
						return false;
				}
				
				//Переименование пользователя.
				sql = String.Format("RENAME USER {0} TO {1}, {0}@localhost TO {1}@localhost", OriginLogin, entryLogin.Text);
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.ExecuteNonQuery();
				QSMain.OnNewStatusText("Ok");
				return true;
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка переименования пользователя!");
				QSMain.ErrorMessage(this,ex);
				return false;
			}
		}
		
		void ChangePassword()
		{
			QSMain.OnNewStatusText("Отправляем новый пароль на сервер...");
			string sql;
			try 
			{
				sql = String.Format("SET PASSWORD FOR {0} = PASSWORD('{1}')", entryLogin.Text, entryPassword.Text);
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.ExecuteNonQuery();
				sql = String.Format("SET PASSWORD FOR {0}@localhost = PASSWORD('{1}')", entryLogin.Text, entryPassword.Text);
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.ExecuteNonQuery();
				QSMain.OnNewStatusText("Пароль изменен. Ok");
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка установки пароля!");
				QSMain.ErrorMessage(this,ex);
			}
		}
		
		void UpdatePrivileges()
		{
			QSMain.OnNewStatusText("Устанавливаем права...");
			try 
			{
				string privileges;
				if(checkAdmin.Active)
					privileges = "ALL";
				else
					privileges = "SELECT, INSERT, UPDATE, DELETE, EXECUTE, SHOW VIEW";
				string sql = "GRANT " + privileges + " ON " + QSMain.connectionDB.Database +".* TO @login";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@login", entryLogin.Text);
				cmd.ExecuteNonQuery();
				if(checkAdmin.Active)
				{
					sql = "GRANT CREATE USER, GRANT OPTION ON *.* TO " + entryLogin.Text + ", " + entryLogin.Text + "@localhost";
				}
				else
				{
					sql = "REVOKE CREATE USER, GRANT OPTION ON *.* FROM " + entryLogin.Text + ", " + entryLogin.Text + "@localhost";
				}
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.ExecuteNonQuery();
				if(checkAdmin.Active)
				{
					sql = "GRANT SELECT, UPDATE ON mysql.* TO " + entryLogin.Text + ", " + entryLogin.Text + "@localhost";
				}
				else
				{
					sql = "REVOKE SELECT, UPDATE ON mysql.* FROM " + entryLogin.Text + ", " + entryLogin.Text + "@localhost";
				}
				cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.ExecuteNonQuery();
				QSMain.OnNewStatusText("Права установлены. Ok");
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка установки прав!");
				QSMain.ErrorMessage(this,ex);
			}
		}
	}
}