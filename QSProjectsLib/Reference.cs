using System;
using System.Collections.Generic;
using Gtk;
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
		public object[] SelectedRow;
		public string SqlSelect;
		public List<ColumnInfo> Columns;
		public int NameColumn = 1;
		public int ParentId = -1;
		public string ParentFieldName = "";
		bool NewNode;
		bool RefChanged = false;
		bool DescriptionField = false;
		
		Gtk.ListStore RefListStore;
		Gtk.TreeModelFilter filter;
		
		Dialog editNode;
		Entry inputNameEntry, inputDiscriptionEntry;
		Label LableName, LableDescription;
		
		public Reference ( bool WithDiscription = false)
		{
			this.Build ();
			this.Destroyed += OnDestroyed;

			DescriptionField = WithDiscription;

			Columns = new List<ColumnInfo>();
			Columns.Add( new ColumnInfo("Код", "{0}", false));
			Columns.Add( new ColumnInfo("Название", "{1}"));
			if(WithDiscription)
			{
				SqlSelect = "SELECT id, name, description FROM @tablename ";
				Columns.Add( new ColumnInfo("Описание", "{2}"));
			}
			else
			{
				SqlSelect = "SELECT id, name FROM @tablename ";
			}
				
		}

		// Событие запуска окна справочника
		public class RunReferenceItemDlgEventArgs : EventArgs
		{
			public string TableName { get; set; }
			public int ItemId { get; set; }
			public int ParentId { get; set; }
			public bool NewItem { get; set; }
			public ResponseType Result { get; set; }
		}
		
		internal static ResponseType OnRunReferenceItemDlg(string TableName, bool New, int id, int parentid)
		{
			ResponseType Result = ResponseType.None;
			EventHandler<RunReferenceItemDlgEventArgs> handler = RunReferenceItemDlg;
			if (handler != null)
			{
				RunReferenceItemDlgEventArgs e = new RunReferenceItemDlgEventArgs();
				e.TableName = TableName;
				e.ItemId = id;
				e.NewItem = New;
				e.ParentId = parentid;
				handler(null, e);
				Result = e.Result;
			}
			return Result;
		}

		public bool ReferenceIsChanged
		{
			get{ return RefChanged;}
		}

		public class ColumnInfo
		{
			/// <summary>
			/// Если имя = пустая строка. Колонка не отображается.
			/// </summary>
			public string Name;
			public bool Search;
			public string DisplayFormat;

			public ColumnInfo(string name, string format, bool search = true)
			{
				Name = name;
				DisplayFormat = format;
				Search = search;
			}
		}

		private bool FilterTree (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			if (entryFilter.Text == "")
				return true;

			string Str;
			for(int i = 0; i < Columns.Count; i++)
			{
				if(Columns[i].Search)
				{
					Str = model.GetValue(iter, i).ToString();
					if (Str.IndexOf (entryFilter.Text, StringComparison.CurrentCultureIgnoreCase) > -1)
						return true;
				}
			}
			return false;
		}
		
		public void FillList(string table, string Nodename, string Refname)
		{
			nameNode = Nodename;
			nameRef = Refname;
			TableRef = table;

			if (RefListStore == null)
				CreateTable();
			
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
			entryFilter.Text = "";
			try
			{
				string sql = SqlSelect.Replace("@tablename", TableRef);
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				MySqlDataReader rdr = cmd.ExecuteReader();
				
				RefListStore.Clear();
				object[] Values = new object[Columns.Count];
				while (rdr.Read())
				{
					Values[0] = rdr.GetInt32(0);
					object[] Fields = new object[rdr.FieldCount];
					rdr.GetValues(Fields);
					for(int i = 1; i < Columns.Count; i++)
					{
						Values[i] = String.Format(Columns[i].DisplayFormat, Fields);
					}
					RefListStore.AppendValues(Values);
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
			addAction1.Sensitive = CanNew;
			editAction1.Sensitive = false;
			removeAction1.Sensitive = false;
		}

		private void CreateTable()
		{
			//Создаем таблицу "Справочника"
			//Первая колонка всегда ID
			System.Type[] Types = new System.Type[Columns.Count];
			Types[0] =	typeof (int); 

			for(int i = 1; i < Columns.Count; i++)
			{
				Types[i] = typeof(string);
			}

			RefListStore = new Gtk.ListStore (Types);

			for(int i = 0; i < Columns.Count; i++)
			{
				if(Columns[i].Name != "")
					treeviewref.AppendColumn (Columns[i].Name , new Gtk.CellRendererText (), "text", i);
			}

			filter = new Gtk.TreeModelFilter (RefListStore, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTree);
			treeviewref.Model = filter;
			treeviewref.ShowAll();
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
				result = OnRunReferenceItemDlg (TableRef, true, -1, ParentId);
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
					if(ParentFieldName == "")
						InsertDescriptString = " (name, description) VALUES (@name, @descript)";
					else
						InsertDescriptString = " (name, description, " + ParentFieldName + ") VALUES (@name, @descript, @parent)";
					UpdateDescriptString = ", description = @descript ";
				}
				else
				{
					if(ParentFieldName == "")
						InsertDescriptString = " (name) VALUES (@name)";
					else
						InsertDescriptString = " (name, " + ParentFieldName + ") VALUES (@name, @parent)";
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
					if(ParentFieldName != "")
						cmd.Parameters.AddWithValue("@parent", ParentId);
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
				result = OnRunReferenceItemDlg (TableRef, false, SelectedID, ParentId);
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
			SelectedID = (int) filter.GetValue(iter,0);
			SelectedName = filter.GetValue(iter, NameColumn).ToString();
			SelectedRow = new object[filter.NColumns];
			for(int i = 0; i < filter.NColumns; i++)
			{
				SelectedRow[i] = filter.GetValue(iter, i);
			}
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
			Delete winDelete = new Delete();
			TreeIter iter;
			treeviewref.Selection.GetSelected(out iter);
			SelectedID = (int) filter.GetValue(iter,0);
			if(winDelete.RunDeletion(TableRef, SelectedID))
			{
				UpdateList();
				RefChanged = true;
			}
			winDelete.Destroy();
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

