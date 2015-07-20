using System;
using Gtk;
using System.Collections.Generic;
using Nini.Config;

namespace QSProjectsLib
{
	public partial class EditConnection : Gtk.Dialog
	{
		List<Connection> connections;
		Gtk.ListStore connectionsListStore;
		Gtk.TreeViewColumn connectionColumn;
		Gtk.CellRendererText connectionCell;
		TreeIter iter;
		IniConfigSource config;
		IConfig defaultConfig;
		bool doNotCheck = false;

		enum connectionStoreCol
		{
			Name,
			Server,
			Base,
			User,
			Section,
			NumberInArray,
			AccountLogin,
			ConnectionType
		}

		public EditConnection (ref List<Connection> Connections, ref IniConfigSource Config)
		{
			this.Build ();
			this.Title = "Настройка соединений";
			connections = Connections;
			config = Config;
			defaultConfig = (IConfig)config.Configs ["Default"];
			labelInfo.ModifyFg (StateType.Normal, new Gdk.Color (255, 0, 0));
			entryLogin.Visible = labelLogin.Visible = labelTitle.Visible = false;
			comboConnectionType.Active = 0;

			connectionsListStore = new Gtk.ListStore (typeof(string),  //0 - Name
			                                          typeof(string),  //1 - Server
			                                          typeof(string),  //2 - Base
			                                          typeof(string),  //3 - User
			                                          typeof(string),  //4 - Section
			                                          typeof(int),     //5 - Number in array
			                                          typeof(string),  //6 - Account login
			                                          typeof(ConnectionType));//7 - Connection type
			treeviewConnections.Model = connectionsListStore;
			connectionColumn = new Gtk.TreeViewColumn ();
			connectionColumn.Title = "Соединения";
			connectionColumn.Visible = true;
			connectionCell = new CellRendererText ();
			connectionCell.Editable = false;
			connectionColumn.PackStart (connectionCell, true);
			connectionColumn.AddAttribute (connectionCell, "text", 0);

			treeviewConnections.AppendColumn (connectionColumn);

			for (int i = 0; i < connections.Count; i++)
				connectionsListStore.AppendValues (connections [i].ConnectionName, 
				                                   connections [i].Server, 
				                                   connections [i].BaseName, 
				                                   connections [i].UserName, 
				                                   connections [i].IniName,
				                                   i,
				                                   connections [i].AccountLogin,
				                                   connections [i].Type);
				                                   
			connectionsListStore.GetIterFirst (out iter);
			entryName.Text = (string)connectionsListStore.GetValue (iter, 0);
			if ((ConnectionType)connectionsListStore.GetValue (iter, 7) == ConnectionType.MySQL)
				entryServer.Text = (string)connectionsListStore.GetValue (iter, 1);
			else {
				comboConnectionType.Active = 1;
				entryLogin.Text = (string)connectionsListStore.GetValue (iter, 6);
			}
			entryBase.Text = (string)connectionsListStore.GetValue (iter, 2);
			treeviewConnections.Selection.SelectIter (iter);
			treeviewConnections.Selection.Changed += HandleChanged;
		}

		void HandleChanged (object sender, EventArgs e)
		{
			TreeIter TempIter;
			treeviewConnections.Selection.GetSelected (out TempIter);
			treeviewConnections.ActivateRow (treeviewConnections.Model.GetPath (TempIter), treeviewConnections.Columns [0]);

		}

		protected void OnTreeviewConnectionsRowActivated (object o, RowActivatedArgs args)
		{
			if (doNotCheck || (!doNotCheck && ValidateAndSave ())) {
				connectionsListStore.GetIterFromString (out iter, args.Path.ToString ());
				entryName.Text = (string)connectionsListStore.GetValue (iter, 0);
				if ((ConnectionType)connectionsListStore.GetValue (iter, 7) == ConnectionType.MySQL) {
					comboConnectionType.Active = 0;
					entryLogin.Text = String.Empty;
					entryServer.Text = (string)connectionsListStore.GetValue (iter, 1);
				} else {
					comboConnectionType.Active = 1;
					entryServer.Text = String.Empty;
					entryLogin.Text = (string)connectionsListStore.GetValue (iter, 6);
				}
				entryBase.Text = (string)connectionsListStore.GetValue (iter, 2);
			}
			treeviewConnections.Selection.SelectIter (iter);

		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (!ValidateAndSave ())
				return;
			connections.RemoveAll (m => m == null);
			this.Respond (ResponseType.Ok);
		}

		protected bool ValidateAndSave ()
		{
			labelInfo.Text = "";
			bool ok = true;
			if (connectionsListStore.GetValue (iter, 0) == null)
				return true;
			int i = (int)connectionsListStore.GetValue (iter, 5);
			for (int j = 0; j < connections.Count; j++) {
				if (connections [j] == null)
					continue;
				if (connections [j].ConnectionName == entryName.Text && j != i) {
					labelInfo.Text += "Подключение с таким названием уже существует.\n";
					ok = false;
					break;
				}
			}
			if (entryName.Text.Replace (" ", "") == String.Empty) {
				labelInfo.Text += "Название подключения не заполнено.\n";
				ok = false;
			}
			if (entryServer.Text.Replace (" ", "") == String.Empty && comboConnectionType.Active == 0) {
				labelInfo.Text += "Адрес сервера не заполнен.\n";
				ok = false;
			}
			if (entryLogin.Text.Replace (" ", "") == String.Empty && comboConnectionType.Active == 1) {
				labelInfo.Text += "Учетная запись не заполнена.\n";
				ok = false;
			}
			if (entryBase.Text.Replace (" ", "") == String.Empty) {
				labelInfo.Text += "Название базы не заполнено.";
				ok = false;
			}
				
			if (ok) {
				//Если соединение уже есть в конфиге - обновляем
				if (i != -1) {
					connections [i].BaseName = entryBase.Text;
					if (comboConnectionType.Active == 0) {
						connections [i].Type = ConnectionType.MySQL;
						connections [i].Server = entryServer.Text;
						connections [i].AccountLogin = String.Empty;
					} else {
						connections [i].Type = ConnectionType.SaaS;
						connections [i].AccountLogin = entryLogin.Text;
						connections [i].Server = String.Empty;
					}
					if (defaultConfig != null &&
					    connections [i].ConnectionName == defaultConfig.Get ("ConnectionName", String.Empty))
						defaultConfig.Set ("ConnectionName", entryName.Text);
					connections [i].ConnectionName = entryName.Text;
				} 
				//Если соединения нет - записываем и присваеваем ID
				else {
					if (comboConnectionType.Active == 0)
						connections.Add (new Connection (ConnectionType.MySQL, entryName.Text, entryBase.Text, entryServer.Text, "", "", ""));
					else
						connections.Add (new Connection (ConnectionType.SaaS, entryName.Text, entryBase.Text, "", "", "", entryLogin.Text));
					i = connections.Count - 1;
					connectionsListStore.SetValue (iter, 5, i);
					int j = 0;
					while (true) {
						if (connections.Find (m => m.IniName == "Login" + j.ToString ()) == null)
							break;
						j++;
					}
					connectionsListStore.SetValue (iter, 4, "Login" + j.ToString ());
					connections [i].IniName = "Login" + j.ToString ();
				}
				connectionsListStore.SetValue (iter, 0, entryName.Text);

				if (comboConnectionType.Active == 0) {
					connectionsListStore.SetValue (iter, 1, entryServer.Text);
					connectionsListStore.SetValue (iter, 6, String.Empty);
					connectionsListStore.SetValue (iter, 7, ConnectionType.MySQL);
				} else {
					connectionsListStore.SetValue (iter, 1, String.Empty);
					connectionsListStore.SetValue (iter, 6, entryLogin.Text);
					connectionsListStore.SetValue (iter, 7, ConnectionType.SaaS);
				}
				connectionsListStore.SetValue (iter, 2, entryBase.Text);
				String IniSection = connections [i].IniName;
				if (config.Configs [IniSection] == null)
					config.Configs.Add (IniSection);
				config.Configs [IniSection].Set ("ConnectionName", connections [i].ConnectionName);
				config.Configs [IniSection].Set ("Server", connections [i].Server);
				config.Configs [IniSection].Set ("Type", ((int)connections [i].Type).ToString ());
				config.Configs [IniSection].Set ("Account", connections [i].AccountLogin);
				config.Configs [IniSection].Set ("DataBase", connections [i].BaseName);
				config.Save ();
			}
			return ok;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if (ValidateAndSave ()) {
				doNotCheck = true;
				iter = connectionsListStore.AppendValues ("Новое соединение", "", Login.DefaultBase, "", "", -1, "", ConnectionType.MySQL);
				entryBase.Sensitive = entryName.Sensitive = entryServer.Sensitive = entryLogin.Sensitive = true;
				treeviewConnections.ActivateRow (treeviewConnections.Model.GetPath (iter), treeviewConnections.Columns [0]);
				comboConnectionType.Active = 0;
				doNotCheck = false;
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			doNotCheck = true;
			TreeSelection selection = treeviewConnections.Selection;
			if (selection.CountSelectedRows () != 1)
				return;
			selection.GetSelected (out iter);
			int id = (int)connectionsListStore.GetValue (iter, 5);
			treeviewConnections.Selection.Changed -= HandleChanged;
			connectionsListStore.Remove (ref iter);
			treeviewConnections.Selection.Changed += HandleChanged;
			if (id != -1) {
				config.Configs.Remove (config.Configs [connections [id].IniName]);
				connections [id] = null;
				config.Save ();
			}
			connectionsListStore.GetIterFirst (out iter);
			treeviewConnections.ActivateRow (treeviewConnections.Model.GetPath (iter), treeviewConnections.Columns [0]);
			doNotCheck = false;
			labelInfo.Text = "";
			if (connections.Count == connections.FindAll (m => m == null).Count) {
				entryBase.Text = entryName.Text = entryServer.Text = entryLogin.Text = "";
				entryBase.Sensitive = entryName.Sensitive = entryServer.Sensitive = entryLogin.Sensitive = false;
			}
		}

		protected void OnComboConnectionTypeChanged (object sender, EventArgs e)
		{
			if (comboConnectionType.Active == 1) {
				entryServer.Visible = labelServer.Visible = false;
				entryLogin.Visible = labelLogin.Visible = labelTitle.Visible = true;
			} else {
				entryServer.Visible = labelServer.Visible = true;
				entryLogin.Visible = labelLogin.Visible = labelTitle.Visible = false;
			}
		}

		protected void OnEntryServerTextInserted (object o, TextInsertedArgs args)
		{
			try {
				if (entryName.Text == entryServer.Text.Remove (args.Position - 1, args.Length))
					entryName.Text = entryServer.Text;
			} catch {
			}
		}

		protected void OnEntryServerTextDeleted (object o, TextDeletedArgs args)
		{
			try {
				if (entryName.Text.Remove (args.StartPos, args.EndPos - args.StartPos) == entryServer.Text)
					entryName.Text = entryServer.Text; 
			} catch {
			}
		}

		protected void OnEntryServerFocusInEvent (object o, FocusInEventArgs args)
		{
			if (entryName.Text == entryServer.Text)
				entryName.ModifyText (StateType.Normal, new Gdk.Color (0, 152, 190));
		}

		protected void OnEntryServerFocusOutEvent (object o, FocusOutEventArgs args)
		{
			entryName.ModifyText (StateType.Normal, new Gdk.Color (0, 0, 0));
		}

		protected void OnEntryNameChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			treeviewConnections.Selection.GetSelected (out iter);
			connectionsListStore.SetValue (iter, (int)connectionStoreCol.Name, entryName.Text);
		}
	}
}

