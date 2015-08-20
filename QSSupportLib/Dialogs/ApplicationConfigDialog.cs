using System.Collections.Generic;
using Gtk;
using System;
using QSProjectsLib;

namespace QSSupportLib
{
	public partial class ApplicationConfigDialog : Dialog
	{
		Dictionary<string, string> parameters;
		ListStore parametersListStore = new ListStore (typeof(string), typeof(string));

		public ApplicationConfigDialog ()
		{
			this.Build ();

			parameters = new Dictionary<string, string> (MainSupport.BaseParameters.All);

			TreeViewColumn paramName = new TreeViewColumn ();
			paramName.Title = "Название";

			TreeViewColumn paramValue = new TreeViewColumn ();
			paramValue.Title = "Значение";

			treeParameters.AppendColumn ("Название", new CellRendererText (), "text", 0);
			treeParameters.AppendColumn ("Значение", new CellRendererText (), "text", 1);

			(treeParameters.Columns [0].Cells [0] as CellRendererText).Editable = true;
			(treeParameters.Columns [0].Cells [0] as CellRendererText).Edited += KeyEdited;
			(treeParameters.Columns [1].Cells [0] as CellRendererText).Editable = true;
			(treeParameters.Columns [1].Cells [0] as CellRendererText).Edited += ValueEdited;
		
			foreach (var pair in parameters) {
				parametersListStore.AppendValues (pair.Key, pair.Value);
			}

			treeParameters.Model = parametersListStore;

			buttonDelete.Sensitive = false;

			treeParameters.Selection.Changed += (sender, e) => {
				buttonDelete.Sensitive = (treeParameters.Selection.CountSelectedRows () > 0);
			};
		}

		void KeyEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (KeyExists (args.NewText)) {
				MessageDialogWorks.RunWarningDialog ("Параметр с таким названием уже существует.");
				return;
			}
			parametersListStore.GetIter (out iter, new TreePath (args.Path));
			parametersListStore.SetValue (iter, 0, args.NewText);
		}

		void ValueEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			parametersListStore.GetIter (out iter, new TreePath (args.Path));
			parametersListStore.SetValue (iter, 1, args.NewText);
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			//Получаем из листа новые значения
			parameters = new Dictionary<string, string> ();
			TreeIter iter = new TreeIter ();
			if (!parametersListStore.GetIterFirst (out iter))
				return;
			do {
				string key = (string)parametersListStore.GetValue (iter, 0);
				string value = (string)parametersListStore.GetValue (iter, 1);
				parameters.Add (key, value);
			} while (parametersListStore.IterNext (ref iter));
			//Добавляем или обновляем.
			foreach (var pair in parameters) {
				string value = String.Empty;
				if (MainSupport.BaseParameters.All.TryGetValue (pair.Key, out value)) {
					if (value == pair.Value)
						continue;
				}
				MainSupport.BaseParameters.UpdateParameter (QSMain.ConnectionDB, pair.Key, pair.Value);
			}
			//Удаляем
			foreach (var pair in MainSupport.BaseParameters.All) {
				if (!parameters.ContainsKey (pair.Key))
					MainSupport.BaseParameters.RemoveParameter (QSMain.ConnectionDB, pair.Key);
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			TreeIter iter = new TreeIter ();
			if (!treeParameters.Selection.GetSelected (out iter))
				return;
			parametersListStore.Remove (ref iter);
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if (KeyExists ("new_parameter")) {
				MessageDialogWorks.RunWarningDialog ("Параметр с таким названием уже существует.");
				return;
			}
			parametersListStore.AppendValues ("new_parameter", "new_value");
		}

		bool KeyExists (string key)
		{
			TreeIter iter = new TreeIter ();
			if (!parametersListStore.GetIterFirst (out iter))
				return false;
			do {
				if ((string)parametersListStore.GetValue (iter, 0) == key)
					return true;
			} while (parametersListStore.IterNext (ref iter));
			return false;
		}
	}
}

