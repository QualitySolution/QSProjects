using System;
using System.Collections.Generic;
using Gtk;
using QSMachineConfig;

namespace QSProjectsLib
{
	public partial class EditConnection : Dialog
	{
		ListStore connectionsListStore = new ListStore (typeof(string), typeof(Connection));
		List<string> sectionsToDelete = new List<string> ();
		List<Connection> connections;
		TreeIter currentIter;
		string lastEdited;

		public event EventHandler EditingDone;

		protected virtual void OnEditingDone ()
		{
			if (EditingDone != null)
				EditingDone (null, EventArgs.Empty);
		}

		public EditConnection (List<Connection> Connections)
		{
			this.Build ();
			this.Title = "Настройка соединений";
			connections = Connections;
		
			labelInfo.ModifyFg (StateType.Normal, new Gdk.Color (255, 0, 0));
			entryLogin.Visible = labelLogin.Visible = labelTitle.Visible = false;
			comboConnectionType.Active = 0;

			//Creating connections treeview
			treeConnections.Model = connectionsListStore;

			TreeViewColumn connectionColumn = new TreeViewColumn ();
			connectionColumn.Title = "Соединения";
			CellRendererText connectionCell = new CellRendererText ();
			connectionCell.Editable = false;
			connectionColumn.PackStart (connectionCell, true);
			connectionColumn.AddAttribute (connectionCell, "text", 0);

			treeConnections.AppendColumn (connectionColumn);

			//Filling in info
			for (int i = 0; i < Connections.Count; i++)
				connectionsListStore.AppendValues (Connections [i].ConnectionName, Connections [i]);

			//Selecting first node and filling in its information
			var conn = Connections [0];
			entryName.Text = conn.ConnectionName;
			entryBase.Text = conn.BaseName;
			if (conn.Type == ConnectionType.MySQL) {
				comboConnectionType.Active = 0;
				entryLogin.Text = String.Empty;
				entryServer.Text = conn.Server;
			} else {
				comboConnectionType.Active = 1;
				entryServer.Text = String.Empty;
				entryLogin.Text = conn.AccountLogin;
			}

			buttonCreateBase.Sensitive = DBCreator.Scripts.Count > 0;

			treeConnections.Model.GetIterFirst (out currentIter);
			treeConnections.Selection.SelectIter (currentIter);
			treeConnections.Selection.Changed += HandleChanged;

			buttonHelp.Visible = !String.IsNullOrEmpty(Login.CreateDBHelpUrl);
			buttonHelp.TooltipText = Login.CreateDBHelpTooltip;
		}

		void HandleChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			treeConnections.Selection.GetSelected (out iter);
			treeConnections.ActivateRow (treeConnections.Model.GetPath (iter), treeConnections.Columns [0]);
		}

		protected void OnTreeviewConnectionsRowActivated (object o, RowActivatedArgs args)
		{
			//Validating current connection before switching to other one.
			if (!CheckAndSaveBeforeSwitch ()) {
				treeConnections.Selection.SelectIter (currentIter);
				return;
			}
			TreeIter iter;
			treeConnections.Model.GetIterFromString (out iter, args.Path.ToString ());

			var conn = (Connection)connectionsListStore.GetValue (iter, 1);
			entryName.Text = conn.ConnectionName;
			entryBase.Text = conn.BaseName;
			if (conn.Type == ConnectionType.MySQL) {
				comboConnectionType.Active = 0;
				entryLogin.Text = String.Empty;
				entryServer.Text = conn.Server;
			} else {
				comboConnectionType.Active = 1;
				entryServer.Text = String.Empty;
				entryLogin.Text = conn.AccountLogin;
			}

			currentIter = iter;
		}

		protected bool CheckAndSaveBeforeSwitch ()
		{
			//If iter is null - row is removed and there is nothing to check here.
			if (currentIter.Equals (TreeIter.Zero))
				return true;
			labelInfo.Text = String.Empty;
			//Checking current input
			if (String.IsNullOrWhiteSpace (entryName.Text)) {
				labelInfo.Text += "Название подключения не заполнено.\n";
				return false;
			}
			if (String.IsNullOrWhiteSpace (entryServer.Text) && comboConnectionType.Active == 0) {
				labelInfo.Text += "Адрес сервера не заполнен.\n";
				return false;
			}
			if (String.IsNullOrWhiteSpace (entryLogin.Text) && comboConnectionType.Active == 1) {
				labelInfo.Text += "Учетная запись не заполнена.\n";
				return false;
			}
			if (String.IsNullOrWhiteSpace (entryBase.Text)) {
				labelInfo.Text += "Название базы не заполнено.";
				return false;
			}

			TreeIter iter;
			if (!treeConnections.Model.GetIterFirst (out iter))
				return false;
			do {
				if (iter.Equals (currentIter))
					continue;
				var conn = (Connection)treeConnections.Model.GetValue (iter, 1);
				if (conn.ConnectionName == entryName.Text) {
					labelInfo.Text += "Подключение с таким названием уже существует.\n";
					return false;
				}
			} while (treeConnections.Model.IterNext (ref iter));

			//If everything is OK - saving updated values to treestore.
			var connection = (Connection)treeConnections.Model.GetValue (currentIter, 1);
			if (connection == null)
				return false;
			connection.ConnectionName = entryName.Text;
			connection.BaseName = entryBase.Text;
			connection.Type = (comboConnectionType.Active == 0 ? ConnectionType.MySQL : ConnectionType.SaaS);
			if (connection.Type == ConnectionType.MySQL) {
				connection.Server = entryServer.Text;
				connection.AccountLogin = String.Empty;
			} else {
				connection.Server = String.Empty;
				connection.AccountLogin = entryLogin.Text;
			}
			treeConnections.Model.SetValue (currentIter, 0, connection.ConnectionName);
			treeConnections.Model.SetValue (currentIter, 1, connection);
			lastEdited = connection.ConnectionName;
			return true;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (!CheckAndSaveBeforeSwitch ())
				return;
			Delete ();
			Save ();
			OnEditingDone ();
			this.Respond (ResponseType.Ok);
		}

		protected void Delete ()
		{
			foreach (string section in sectionsToDelete) {
				var config = MachineConfig.ConfigSource.Configs [section];
				if (config != null)
					MachineConfig.ConfigSource.Configs.Remove (config);
			}
			MachineConfig.ConfigSource.Save ();
		}

		protected void Save ()
		{
			TreeIter iter;
			if (!treeConnections.Model.GetIterFirst (out iter))
				return;
			do {
				var connection = (Connection)treeConnections.Model.GetValue (iter, 1);
				if (String.IsNullOrWhiteSpace (connection.IniName)) {
					int i = 0;
					for (;; i++) {
						if (connections.Find (m => m.IniName == ("Login" + i)) == null)
							break;
					}
					connection.IniName = "Login" + i;
				}
				var section = connection.IniName;
				if (MachineConfig.ConfigSource.Configs [section] == null)
					MachineConfig.ConfigSource.Configs.Add (section);
				MachineConfig.ConfigSource.Configs [section].Set ("ConnectionName", connection.ConnectionName);
				MachineConfig.ConfigSource.Configs [section].Set ("Server", connection.Server);
				MachineConfig.ConfigSource.Configs [section].Set ("Type", ((int)connection.Type).ToString ());
				MachineConfig.ConfigSource.Configs [section].Set ("Account", connection.AccountLogin);
				MachineConfig.ConfigSource.Configs [section].Set ("DataBase", connection.BaseName);
			} while (treeConnections.Model.IterNext (ref iter));

			if (MachineConfig.ConfigSource.Configs ["Default"] == null)
				MachineConfig.ConfigSource.AddConfig ("Default");
			MachineConfig.ConfigSource.Configs ["Default"].Set ("ConnectionName", lastEdited);
			
			MachineConfig.ConfigSource.Save ();	
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			if (!CheckAndSaveBeforeSwitch ())
				return;
			iter = connectionsListStore.AppendValues ("Новое соединение", 
				new Connection (ConnectionType.MySQL,
					"Новое соединение",
					Login.DefaultBase,
					"", "", "", ""));
			treeConnections.Selection.SelectIter (iter);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (treeConnections.Selection.CountSelectedRows () != 1)
				return;
			TreeIter iter;
			treeConnections.Selection.GetSelected (out iter);
			sectionsToDelete.Add (((Connection)connectionsListStore.GetValue (iter, 1)).IniName);
			treeConnections.Selection.Changed -= HandleChanged;
			connectionsListStore.Remove (ref iter);
			treeConnections.Selection.Changed += HandleChanged;
			currentIter = TreeIter.Zero;
			connectionsListStore.GetIterFirst (out iter);
			treeConnections.Selection.SelectIter (iter);
		}

		protected void OnComboConnectionTypeChanged (object sender, EventArgs e)
		{
			if (comboConnectionType.Active == 1) {
				entryServer.Visible = labelServer.Visible = hboxCreateBase.Visible = false;
				entryLogin.Visible = labelLogin.Visible = labelTitle.Visible = true;
			} else {
				entryServer.Visible = labelServer.Visible = hboxCreateBase.Visible = true;
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
			treeConnections.Selection.GetSelected (out iter);
			connectionsListStore.SetValue (iter, 0, entryName.Text);
		}

		protected void OnButtonCreateBaseClicked(object sender, EventArgs e)
		{
			DBCreator.RunCreation (entryServer.Text, entryBase.Text);
		}

		protected void OnButtonHelpClicked(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(Login.CreateDBHelpUrl);
		}
	}
}

