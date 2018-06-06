using System;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;

namespace QSProjectsLib
{
	public partial class Users : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		Gtk.ListStore UsersListStore;

		public Users ()
		{
			this.Build ();
			
			//Создаем таблицу "Пользователей"
			UsersListStore = new Gtk.ListStore (typeof(int), typeof(string), typeof(string), 
				typeof(bool), typeof(bool), typeof(string));
			
			treeviewUsers.AppendColumn ("Код", new Gtk.CellRendererText (), "text", 0);
			treeviewUsers.AppendColumn ("Логин", new Gtk.CellRendererText (), "text", 1);
			treeviewUsers.AppendColumn ("Имя", new Gtk.CellRendererText (), "text", 2);
			treeviewUsers.AppendColumn ("Администратор", new Gtk.CellRendererToggle (), "active", 3);
			foreach (var col in treeviewUsers.Columns) {
				if(col.CellRenderers [0] is CellRendererText)
					col.AddAttribute (col.CellRenderers [0], "foreground", 5);
			}
			treeviewUsers.Model = UsersListStore;
			treeviewUsers.ShowAll ();
			UpdateUsers ();
		}

		void UpdateUsers ()
		{
			if (!QSMain.TestConnection ())
				return;
			QSMain.CheckConnectionAlive ();
			logger.Info ("Получаем таблицу пользователей...");
			
			string sql = "SELECT * FROM users ";
			if(!chkShowInactive.Active)
				sql += " WHERE deactivated = 0";
			MySqlCommand cmd = new MySqlCommand (sql, QSMain.connectionDB);
			
			MySqlDataReader rdr = cmd.ExecuteReader ();
			
			UsersListStore.Clear ();
			while (rdr.Read ()) {
				bool deactivated = false;
				try {
					deactivated = DBWorks.GetBoolean (rdr, "deactivated", false);
				} catch {
					logger.Warn ("В базе отсутствует поле deactivated!");
				}
				UsersListStore.AppendValues (int.Parse (rdr ["id"].ToString ()),
					rdr ["login"].ToString (),
					rdr ["name"].ToString (),
					(bool)rdr ["admin"],
					deactivated,
					deactivated ? "grey" : "black");
			}
			rdr.Close ();
			
			logger.Info ("Ok");
			
			OnTreeviewUsersCursorChanged (null, null);
		}

		protected void OnTreeviewUsersCursorChanged (object sender, EventArgs e)
		{
			bool isSelect = treeviewUsers.Selection.CountSelectedRows () == 1;
			buttonEdit.Sensitive = isSelect;
			buttonDelete.Sensitive = isSelect;
		}

		protected void OnTreeviewUsersRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			UserProperty winUser = new UserProperty ();
			winUser.NewUser = true;
			winUser.Show ();
			winUser.Run ();
			winUser.Destroy ();
			UpdateUsers ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			
			treeviewUsers.Selection.GetSelected (out iter);
			int itemid = Convert.ToInt32 (UsersListStore.GetValue (iter, 0));
			UserProperty winUser = new  UserProperty ();
			winUser.UserFill (itemid);
			winUser.Show ();
			winUser.Run ();
			winUser.Destroy ();
			UpdateUsers ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			
			treeviewUsers.Selection.GetSelected (out iter);
			int itemid = (int)UsersListStore.GetValue (iter, 0);
			string loginname = UsersListStore.GetValue (iter, 1).ToString ();
			bool result;
			if (QSMain.IsOrmDeletionConfigered) {
				result = QSMain.OnOrmDeletion ("users", itemid);
			} else {
				Delete winDel = new Delete ();
				result = winDel.RunDeletion ("users", itemid);
				winDel.Destroy ();
			}

			if (result) {
				logger.Info ("Удаляем пользователя MySQL...");
				if (QSSaaS.Session.IsSaasConnection) {
					QSSaaS.ISaaSService svc = QSSaaS.Session.GetSaaSService ();
					if (!svc.changeBaseAccessFromProgram (QSSaaS.Session.SessionId, loginname, QSSaaS.Session.SaasBaseName, false))
						logger.Error ("Ошибка удаления доступа к базе на сервере SaaS.");
				} else {
					string sql;
					sql = "DROP USER @login, @login @'localhost'";
					try {
						QSMain.CheckConnectionAlive ();
						MySqlCommand cmd = new MySqlCommand (sql, QSMain.connectionDB);
						cmd.Parameters.AddWithValue ("@login", loginname);
						cmd.ExecuteNonQuery ();
						logger.Info ("Пользователь удалён. Ok");

						if (QSMain.User.Id == itemid) {
							MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent,
								                   MessageType.Warning, ButtonsType.Close,
								                   "Был удален пользователь, под которым Вы подключились к базе данных, чтобы недопустить некорректных операций программа закроется. Зайдите в программу от имени другого пользователя.");
							md.Run ();
							md.Destroy ();
							Environment.Exit (0);
						}
					} catch (Exception ex) {
						logger.Error (ex, "Ошибка удаления пользователя!");
						QSMain.ErrorMessage (this, ex);
					}
				}
			}

			UpdateUsers ();
		}

		protected void OnChkShowInactiveToggled(object sender, EventArgs e)
		{
			UpdateUsers();
		}
	}
}