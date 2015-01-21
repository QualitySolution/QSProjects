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
		bool doNotCheck = false;

		public EditConnection(ref List<Connection> Connections, ref IniConfigSource Config)
		{
			this.Build ();
			this.Title = "Настройка соединений";
			connections = Connections;
			config = Config;
			labelInfo.ModifyFg(StateType.Normal, new Gdk.Color(255,0,0));

			connectionsListStore = new Gtk.ListStore (typeof (string),  //Name
			                                          typeof (string),  //Server
			                                          typeof (string),  //Base
			                                          typeof (string),  //User
			                                          typeof (string),  //Section
			                                          typeof (int));    //Number in array
			treeviewConnections.Model = connectionsListStore;
			connectionColumn = new Gtk.TreeViewColumn ();
			connectionColumn.Title = "Соединения";
			connectionColumn.Visible = true;
			connectionCell = new CellRendererText ();
			connectionCell.Editable = false;
			connectionColumn.PackStart (connectionCell, true);
			connectionColumn.AddAttribute(connectionCell, "text", 0);

			treeviewConnections.AppendColumn (connectionColumn);

			for (int i = 0; i < connections.Count; i++)
				connectionsListStore.AppendValues (connections[i].ConnectionName, 
				                                   connections[i].Server, 
				                                   connections[i].BaseName, 
				                                   connections[i].UserName, 
				                                   connections[i].IniName,
				                                   i);
				                                   
			connectionsListStore.GetIterFirst (out iter);
			entryName.Text = (string)connectionsListStore.GetValue (iter, 0);
			entryServer.Text = (string)connectionsListStore.GetValue (iter, 1);
			entryBase.Text = (string)connectionsListStore.GetValue (iter, 2);
			treeviewConnections.Selection.SelectIter (iter);
			treeviewConnections.Selection.Changed += HandleChanged;
		}

		void HandleChanged (object sender, EventArgs e)
		{
			TreeIter TempIter;
			treeviewConnections.Selection.GetSelected(out TempIter);
			treeviewConnections.ActivateRow (treeviewConnections.Model.GetPath (TempIter), treeviewConnections.Columns [0]);

		}
	
		protected void OnTreeviewConnectionsRowActivated (object o, RowActivatedArgs args)
		{
			if (doNotCheck || (!doNotCheck && ValidateAndSave())) {
					connectionsListStore.GetIterFromString (out iter, args.Path.ToString ());
					entryName.Text = (string)connectionsListStore.GetValue (iter, 0);
					entryServer.Text = (string)connectionsListStore.GetValue (iter, 1);
					entryBase.Text = (string)connectionsListStore.GetValue (iter, 2);
			}
			treeviewConnections.Selection.SelectIter (iter);

		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (!ValidateAndSave ())
				return;
			//connections.RemoveAll (m => m == null);
			this.Respond (ResponseType.Ok);
		}

		protected bool ValidateAndSave ()
		{
			labelInfo.Text = "";
			bool ok = true;
			if (connectionsListStore.GetValue (iter, 0) == null)
				return true;
			int i = (int)connectionsListStore.GetValue(iter, 5);
			for (int j = 0; j < connections.Count; j++) {
				if (connections [j] == null)
					continue;
				if (connections [j].ConnectionName == entryName.Text && j != i) {
					labelInfo.Text += "Подключение с таким названием уже существует.\n";
					ok = false;
					break;
				}
			}
			if (entryName.Text == String.Empty) {
				labelInfo.Text += "Название подключения не заполнено.\n";
				ok = false;
			}
			if (entryServer.Text == String.Empty) {
				labelInfo.Text += "Адрес сервера не заполнен.\n";
				ok = false;
			}
			if (entryBase.Text == String.Empty) {
				labelInfo.Text += "Название базы не заполнено.";
				ok = false;
			}
			if (ok) {
				if (i != -1) {
					connections [i].BaseName = entryBase.Text;
					connections [i].Server = entryServer.Text;
					connections [i].ConnectionName = entryName.Text;
				} else {
					connections.Add (new Connection (entryName.Text, entryBase.Text, entryServer.Text, "", ""));
					i = connections.Count - 1;
					connectionsListStore.SetValue (iter, 5, i);
					int j = 0;
					while (true) {
						if (connections.Find (m => m.IniName == "Login" + j.ToString ()) == null)
							break;
						j++;
					}
					connectionsListStore.SetValue (iter, 4, "Login" + j.ToString());
					connections [i].IniName = "Login" + j.ToString ();
				}
				connectionsListStore.SetValue (iter, 0, entryName.Text);
				connectionsListStore.SetValue (iter, 1, entryServer.Text);
				connectionsListStore.SetValue (iter, 2, entryBase.Text);
				String IniSection = connections[i].IniName;
				if (config.Configs[IniSection] == null)
					config.Configs.Add (IniSection);
				config.Configs [IniSection].Set ("ConnectionName", connections[i].ConnectionName);
				config.Configs [IniSection].Set ("Server", connections[i].Server);
				config.Configs [IniSection].Set ("DataBase", connections[i].BaseName);
				config.Save ();
			}
			return ok;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if (ValidateAndSave ()) {
				doNotCheck = true;
				iter = connectionsListStore.AppendValues ("Новое соединение", "", "", "", "", -1);
				entryBase.Sensitive = entryName.Sensitive = entryServer.Sensitive = true;
				treeviewConnections.ActivateRow (treeviewConnections.Model.GetPath (iter), treeviewConnections.Columns [0]);
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
			connectionsListStore.Remove (ref iter);
			if (id != -1) {
				config.Configs.Remove (config.Configs[connections [id].IniName]);
				connections [id] = null;
				config.Save ();
			}
			connectionsListStore.GetIterFirst (out iter);
			treeviewConnections.ActivateRow (treeviewConnections.Model.GetPath (iter), treeviewConnections.Columns [0]);
			doNotCheck = false;
			labelInfo.Text = "";
			if (connections.Count == connections.FindAll(m => m == null).Count) {
				entryBase.Text = entryName.Text = entryServer.Text = "";
				entryBase.Sensitive = entryName.Sensitive = entryServer.Sensitive = false;
			}
		}
	}
}

