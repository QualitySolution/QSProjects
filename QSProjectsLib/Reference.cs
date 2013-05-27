using System;
using System.Data;
using Gtk;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace QSProjectsLib
{
	public partial class Reference : Gtk.Dialog
	{
		public static event EventHandler<RunReferenceItemDlgEventArgs> RunReferenceItemDlg;

		bool SimpleMode, SelectMode, CanNew, CanEdit, CanDel;
		string TableRef, nameNode, nameRef;
		public int SelectedID;
		public string SelectedName;
		bool NewNode;
		bool RefChanged = false;
		bool DescriptionField = false;
		
		Gtk.ListStore RefListStore;
		Gtk.TreeModelFilter filter;
		
		Gtk.TreeViewColumn DescriptionColumn;
		
		Dialog editNode;
		Entry inputNameEntry, inputDiscriptionEntry;
		Label LableName, LableDescription;
		
		public Reference ()
		{
			this.Build ();
			this.Destroyed += OnDestroyed;
			
			//Создаем таблицу "Справочника"
			RefListStore = new Gtk.ListStore (typeof (int), typeof (string), typeof (string));
			
			DescriptionColumn = new Gtk.TreeViewColumn ();
			DescriptionColumn.Title = "Описание";
			DescriptionColumn.Visible = false;
			Gtk.CellRendererText DiscriptCell = new Gtk.CellRendererText ();
			DescriptionColumn.PackStart (DiscriptCell, true);
			
			treeviewref.AppendColumn ("Код", new Gtk.CellRendererText (), "text", 0);
			treeviewref.AppendColumn ("Название", new Gtk.CellRendererText (), "text", 1);
			treeviewref.AppendColumn (DescriptionColumn);
			DescriptionColumn.AddAttribute(DiscriptCell, "text" , 2);
			
			filter = new Gtk.TreeModelFilter (RefListStore, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTree);
			treeviewref.Model = filter;
			treeviewref.ShowAll();
		}

		//Событие запуска окна справочника
		public class RunReferenceItemDlgEventArgs : EventArgs
		{
			public string TableName { get; set; }
			public int ItemId { get; set; }
			public bool NewItem { get; set; }
			public ResponseType Result { get; set; }
		}
		
		internal static ResponseType OnRunReferenceItemDlg(string TableName, bool New, int id)
		{
			ResponseType Result = ResponseType.None;
			EventHandler<RunReferenceItemDlgEventArgs> handler = RunReferenceItemDlg;
			if (handler != null)
			{
				RunReferenceItemDlgEventArgs e = new RunReferenceItemDlgEventArgs();
				e.TableName = TableName;
				e.ItemId = id;
				e.NewItem = New;
				handler(null, e);
				Result = e.Result;
			}
			return Result;
		}

		private bool FilterTree (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			if (entryFilter.Text == "")
				return true;
			
			string Refname = model.GetValue(iter, 1).ToString();
			
			if (Refname.IndexOf (entryFilter.Text, StringComparison.CurrentCultureIgnoreCase) > -1)
				return true;
			else
				return false;
		}
		
		public void FillList(string table, string Nodename, string Refname)
		{
			nameNode = Nodename;
			nameRef = Refname;
			TableRef = table;
			
			if(TableRef == "place_types")
			{
				DescriptionField = true;
				DescriptionColumn.Visible = true;
			}
			
			if(SelectMode)
			{
				this.Title = "Выберите " + nameNode;
			}
			else
			{
				this.Title = nameRef;	
			}
			UpdateList();
		}
		
		protected void UpdateList()
		{
			QSMain.OnNewStatusText("Получаем таблицу справочника "+ nameRef + "...");
			string sql = "SELECT id, name";
			if(DescriptionField)
				sql += ", description";
			sql += " FROM " + TableRef;
			entryFilter.Text = "";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				MySqlDataReader rdr = cmd.ExecuteReader();
				
				RefListStore.Clear();
				while (rdr.Read())
				{
					if(DescriptionField)
						RefListStore.AppendValues(int.Parse(rdr["id"].ToString()), rdr["name"].ToString(), rdr["description"].ToString());
					else
						RefListStore.AppendValues(int.Parse(rdr["id"].ToString()), rdr["name"].ToString());
				}
				rdr.Close();
				QSMain.OnNewStatusText("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка получения таблицы!");
				QSMain.ErrorMessage(this,ex);
			}
			OnTreeviewrefCursorChanged((object)treeviewref,EventArgs.Empty);
		}
		protected virtual void OnEntryFilterChanged (object sender, System.EventArgs e)
		{
			filter.Refilter ();
		}
		
		void OnInputEntryChanged (object sender, System.EventArgs e)
		{
			bool CanSave = inputNameEntry.Text != "";
			editNode.SetResponseSensitive(ResponseType.Ok, CanSave);
		}
		
		public void SetMode(bool Simple, bool Select, bool New, bool Edit, bool Del)
		{
			SelectMode = Select;
			SimpleMode = Simple;
			CanNew = New;
			CanEdit = Edit;
			CanDel = Del;
			
			buttonOk.Visible = Select;
			buttonCancel.Visible = Select;
			buttonClose.Visible = !Select;
			addAction.Sensitive = CanNew;
			editAction.Sensitive = false;
			removeAction.Sensitive = false;
		}
		
		protected virtual void OnAddActionActivated (object sender, System.EventArgs e)
		{
			ResponseType result;
			if(SimpleMode)
			{
				NewNode = true;
				editNode = new Dialog("Новый " + nameNode, this, Gtk.DialogFlags.DestroyWithParent);
				BuildSimpleEditorDialog ();
				editNode.ShowAll();
				result = (ResponseType) editNode.Run ();
				inputNameEntry.Destroy();
				editNode.Destroy ();
			}
			else
			{
				//Вызываем событие в основном приложении для запуска диалога элемента справочника
				result = OnRunReferenceItemDlg (TableRef, true, -1);
			}
			
			if (result == ResponseType.Ok)
			{
				UpdateList();
				RefChanged = true;
			}
		}
		
		void on_editnode_response (object obj, ResponseArgs args)
		{
			if(args.ResponseId == ResponseType.Ok)
			{
				QSMain.OnNewStatusText("Запись " + nameNode + "...");
				string sql, InsertDescriptString, UpdateDescriptString;
				if(DescriptionField)
				{
					InsertDescriptString = " (name, description) VALUES (@name, @descript)";
					UpdateDescriptString = ", description = @descript ";
				}
				else
				{
					InsertDescriptString = " (name) VALUES (@name)";
					UpdateDescriptString = "";
				}
				if(NewNode)
				{
					sql = "INSERT INTO " + TableRef + InsertDescriptString;
				}
				else
				{
					sql = "UPDATE " + TableRef + " SET name = @name " + UpdateDescriptString +
						"WHERE id = @id";
				}
				try 
				{
					MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
					
					cmd.Parameters.AddWithValue("@id",SelectedID);
					cmd.Parameters.AddWithValue("@name", inputNameEntry.Text);
					if(DescriptionField)
						cmd.Parameters.AddWithValue("@descript", inputDiscriptionEntry.Text);
					
					cmd.ExecuteNonQuery();
					QSMain.OnNewStatusText("Ok");
				} 
				catch (Exception ex) 
				{
					Console.WriteLine(ex.ToString());
					QSMain.OnNewStatusText("Ошибка записи "+ nameNode + "!");
					QSMain.ErrorMessage(this,ex);
				}
			}
		}
		protected virtual void OnEditActionActivated (object sender, System.EventArgs e)
		{
			ResponseType result;
			TreeIter iter;
			treeviewref.Selection.GetSelected(out iter);
			SelectedID = Convert.ToInt32(filter.GetValue(iter,0));
			string NameOfNode = filter.GetValue(iter,1).ToString();
			string DiscriptionOfNode;
			if(DescriptionField)
				DiscriptionOfNode = filter.GetValue(iter,2).ToString();
			else
				DiscriptionOfNode = "";
			
			if(SimpleMode)
			{
				NewNode = false;
				editNode = new Dialog("Редактирование" + nameNode, this, Gtk.DialogFlags.DestroyWithParent);
				BuildSimpleEditorDialog ();
				inputNameEntry.Text = NameOfNode;
				inputDiscriptionEntry.Text = DiscriptionOfNode;
				editNode.ShowAll();
				result = (ResponseType)editNode.Run ();
				inputNameEntry.Destroy();
				editNode.Destroy ();
			}
			else
			{
				//Вызываем событие в основном приложении для запуска диалога элемента справочника
				result = OnRunReferenceItemDlg (TableRef, false, SelectedID);
			}
			
			if(result == ResponseType.Ok)
			{
				UpdateList();
				RefChanged = true;
			}
		}
		
		protected virtual void OnTreeviewrefCursorChanged (object sender, System.EventArgs e)
		{
			bool isSelect = treeviewref.Selection.CountSelectedRows() == 1;
			editAction1.Sensitive = isSelect && CanEdit;
			removeAction1.Sensitive = isSelect && CanDel;
			buttonOk.Sensitive = isSelect;
		}
		
		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			treeviewref.Selection.GetSelected(out iter);
			SelectedID = Convert.ToInt32(filter.GetValue(iter,0));
			SelectedName = filter.GetValue(iter,1).ToString();
		}
		
		protected virtual void OnTreeviewrefRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			if(SelectMode)
			{
				buttonOk.Click();
			}
			else
			{
				editAction1.Activate();
			}
		}
		
		protected virtual void OnButtonCleanClicked (object sender, System.EventArgs e)
		{
			entryFilter.Text = "";
		}
		
		protected void OnRemoveActionActivated (object sender, System.EventArgs e)
		{
			//FIXME Удаление
/*			Delete winDelete = new Delete();
			TreeIter iter;
			treeviewref.Selection.GetSelected(out iter);
			SelectedID = Convert.ToInt32(filter.GetValue(iter,0));
			if(winDelete.RunDeletion(TableRef, SelectedID))
			{
				UpdateList();
				RefChanged = true;
			}
			winDelete.Destroy(); */
		}
		
		protected void BuildSimpleEditorDialog()
		{
			editNode.Modal = true;
			editNode.AddButton ("Отмена", ResponseType.Cancel);
			editNode.AddButton ("Ok", ResponseType.Ok);
			Gtk.Table editNodeTable = new Table(2,2,false);
			LableName = new Label ("Название:");
			LableName.Justify = Justification.Right;
			LableDescription = new Label ("Описание:");
			LableDescription.Justify = Justification.Right;
			editNodeTable.Attach(LableName,0,1,0,1);
			inputNameEntry = new Entry();
			inputNameEntry.WidthRequest = 300;
			editNodeTable.Attach(inputNameEntry,1,2,0,1);
			inputDiscriptionEntry = new Entry();
			editNodeTable.Attach(LableDescription,0,1,1,2);
			editNodeTable.Attach(inputDiscriptionEntry,1,2,1,2);
			if(!DescriptionField)
			{
				inputDiscriptionEntry.Sensitive = false;
				LableDescription.Sensitive = false;
			}
			editNode.VBox.Add(editNodeTable);
			editNode.Response += new ResponseHandler (on_editnode_response);
		}
		
		protected void OnDestroyed (object sender, EventArgs e)
		{
			if(RefChanged)
				QSMain.OnReferenceUpdated (TableRef);
		}

	}
}

