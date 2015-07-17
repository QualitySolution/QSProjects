using System;
using Gtk;
using System.Data.Common;
using System.Collections.Generic;
using QSProjectsLib;
using NLog;

namespace QSProjectsLib
{
	public partial class Delete : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		bool ErrorHappens = false;
		string ErrorString;
		
		TreeStore ObjectsTreeStore;

		public Delete ()
		{
			this.Build ();

			ObjectsTreeStore = new TreeStore(typeof(string), typeof(string));
			treeviewObjects.AppendColumn("Объект", new Gtk.CellRendererText (), "text", 0);

			treeviewObjects.Model = ObjectsTreeStore;
			treeviewObjects.ShowAll ();
		}
		
		public bool RunDeletion(string table, int IntKey, string StrKey)
		{
			int CountReferenceItems = 0;
			bool result = false;
			TableInfo.Params OutParam = new TableInfo.Params(IntKey,StrKey);
			try
			{
				CountReferenceItems = FillObjects (table, new TreeIter(), OutParam);
			}
			catch (Exception ex)
			{
				logger.Error(ex, "При заполнении объектов удаления произошла ошибка!");
				ErrorHappens = true;
				ErrorString = ex.ToString ();
			}
			if(ErrorHappens)
			{
				MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent,
				                                      MessageType.Error, 
				                                      ButtonsType.Close,
				                                      "ошибка");
				md.UseMarkup = false;
				md.Text = "При выполнении поиска зависимостей удаляемого объекта произошла ошибка. Убедитесь, что версия базы данных соответствует версии программы. Если версия базы данных правильная, сообщите разработчику об ошибке в программе.\n\n" + ErrorString;
				md.Run ();
				md.Destroy();
				this.Destroy ();
				return false;
			}
			if(CountReferenceItems > 0)
			{
				if(CountReferenceItems < 10)
					treeviewObjects.ExpandAll ();
				this.Title = String.Format("Удалить {0}?", QSMain.ProjectTables[table].ObjectName);
				result = (ResponseType)this.Run () == ResponseType.Yes;
			}
			else
			{
				this.Hide();
				result = SimpleDialog(QSMain.ProjectTables[table].ObjectName) == ResponseType.Yes;
			}
			if(result)
				DeleteObjects (table,OutParam);
			this.Destroy ();
			return result;
		}
		
		/// <summary>
		/// Runs the deletion.
		/// </summary>
		/// <returns><c>true</c>, if deletion was run, <c>false</c> otherwise.</returns>
		/// <param name="table">Table.</param>
		/// <param name="IntKey">Int key.</param>
		public bool RunDeletion(string table, int IntKey)
		{
			return RunDeletion (table, IntKey, "");
		}

		public bool RunDeletion(string table, string StrKey)
		{
			return RunDeletion (table, 0, StrKey);
		}

		ResponseType SimpleDialog(string ObjectName)
		{
			MessageDialog md = new MessageDialog (null, DialogFlags.DestroyWithParent,
	                              MessageType.Question, 
                                  ButtonsType.YesNo,"Вы уверены что хотите удалить "+ ObjectName + "?");
			ResponseType result = (ResponseType)md.Run ();
			md.Destroy();
			return result;
		}

		int FillObjects(string Table, TreeIter root, TableInfo.Params ParametersIn)
		{
			TreeIter DeleteIter, ClearIter, GroupIter, ItemIter;
			int Totalcount = 0;
			int DelCount = 0;
			int ClearCount = 0;
			int GroupCount;
			DbCommand cmd;
			DbDataReader rdr;
			QSMain.CheckConnectionAlive();
			logger.Debug ("Поиск зависимостей для таблицы {0}", Table);
			if(!QSMain.ProjectTables.ContainsKey(Table))
			{
				ErrorString = "Нет описания для таблицы " + Table;
				logger.Error(ErrorString);
				ErrorHappens = true;
				return 0;
			}
			if(QSMain.ProjectTables[Table].DeleteItems.Count > 0)
			{
				if(!ObjectsTreeStore.IterIsValid(root))
					DeleteIter = ObjectsTreeStore.AppendNode();
				else
					DeleteIter = ObjectsTreeStore.AppendNode (root);
				foreach(KeyValuePair<string, TableInfo.DeleteDependenceItem> pair in QSMain.ProjectTables[Table].DeleteItems)
				{
					GroupCount = 0;
					if(!QSMain.ProjectTables.ContainsKey(pair.Key))
					{
						ErrorString = String.Format ("Зависимость удаления у таблицы {1} ссылается на таблицу {0} описания для которой нет.", pair.Key, Table);
						logger.Error(ErrorString);
						ErrorHappens = true;
						return 0;
					}
					string sql = QSMain.ProjectTables[pair.Key].SqlSelect + pair.Value.sqlwhere;
					cmd = QSMain.ConnectionDB.CreateCommand();
					cmd.CommandText = sql;
					AddParameters(cmd, pair.Value.SqlParam, ParametersIn);

					rdr = cmd.ExecuteReader();
					if(!rdr.HasRows)
					{
						rdr.Close ();
						continue;
					}
					GroupIter = ObjectsTreeStore.AppendNode(DeleteIter);
					int IndexIntParam = 0;
					int IndexStrParam = 0;
					if(QSMain.ProjectTables[pair.Key].PrimaryKey.ParamInt != "")
						IndexIntParam = rdr.GetOrdinal(QSMain.ProjectTables[pair.Key].PrimaryKey.ParamInt);
					if(QSMain.ProjectTables[pair.Key].PrimaryKey.ParamStr != "")
						IndexStrParam = rdr.GetOrdinal(QSMain.ProjectTables[pair.Key].PrimaryKey.ParamStr);
					List<object[]> ReadedData = new List<object[]>();
					while(rdr.Read())
					{
						object[] Fields = new object[rdr.FieldCount];
						rdr.GetValues(Fields);
						ReadedData.Add(Fields);
					}
					rdr.Close ();

					foreach(object[] row in ReadedData)
					{
						ItemIter = ObjectsTreeStore.AppendValues(GroupIter, String.Format(QSMain.ProjectTables[pair.Key].DisplayString, row));
						if(QSMain.ProjectTables[pair.Key].DeleteItems.Count > 0 || QSMain.ProjectTables[pair.Key].ClearItems.Count > 0)
						{
							TableInfo.Params OutParam = new TableInfo.Params();
							if(QSMain.ProjectTables[pair.Key].PrimaryKey.ParamInt != "")
								OutParam.ParamInt = Convert.ToInt32(row[IndexIntParam]);
							if(QSMain.ProjectTables[pair.Key].PrimaryKey.ParamStr != "")
								OutParam.ParamStr = row[IndexStrParam].ToString();
							Totalcount += FillObjects (pair.Key,ItemIter,OutParam);
						}
						GroupCount++;
						Totalcount++;
						DelCount++;
					}
					ObjectsTreeStore.SetValues(GroupIter, QSMain.ProjectTables[pair.Key].ObjectsName + "(" + GroupCount.ToString() + ")");
				}
				if(DelCount > 0)
					ObjectsTreeStore.SetValues(DeleteIter, String.Format ("Будет удалено ({0}/{1}) объектов:",DelCount,Totalcount));
				else
					ObjectsTreeStore.Remove (ref DeleteIter);
			}

			if(QSMain.ProjectTables[Table].ClearItems.Count > 0)
			{
				if(!ObjectsTreeStore.IterIsValid(root))
					ClearIter = ObjectsTreeStore.AppendNode();
				else
					ClearIter = ObjectsTreeStore.AppendNode (root);
				foreach(KeyValuePair<string, TableInfo.ClearDependenceItem> pair in QSMain.ProjectTables[Table].ClearItems)
				{
					GroupCount = 0;
					if(!QSMain.ProjectTables.ContainsKey(pair.Key))
					{
						ErrorString = String.Format ("Зависимость очистки у таблицы {1} ссылается на таблицу {0} описания для которой нет.", pair.Key, Table);
						logger.Error(ErrorString);
						ErrorHappens = true;
						return 0;
					}
					string sql = QSMain.ProjectTables[pair.Key].SqlSelect + pair.Value.sqlwhere;
					cmd = QSMain.ConnectionDB.CreateCommand();
					cmd.CommandText = sql;
					AddParameters(cmd, pair.Value.SqlParam, ParametersIn);

					rdr = cmd.ExecuteReader();
					if(!rdr.HasRows)
					{
						rdr.Close ();
						continue;
					}
					GroupIter = ObjectsTreeStore.AppendNode(ClearIter);
					
					while(rdr.Read())
					{
						object[] Fields = new object[rdr.FieldCount];
						rdr.GetValues(Fields);
						ItemIter = ObjectsTreeStore.AppendValues(GroupIter, String.Format(QSMain.ProjectTables[pair.Key].DisplayString,Fields));
						GroupCount++;
						Totalcount++;
						ClearCount++;
					}
					ObjectsTreeStore.SetValues(GroupIter, QSMain.ProjectTables[pair.Key].ObjectsName + "(" + GroupCount.ToString() + ")");
					rdr.Close ();
				}
				if(ClearCount > 0)
					ObjectsTreeStore.SetValues(ClearIter, String.Format ("Будет очищено ссылок у {0} объектов:",ClearCount));
				else
					ObjectsTreeStore.Remove (ref ClearIter);
			}
			return Totalcount;
		}

		private void DeleteObjects(string Table, TableInfo.Params ParametersIn)
		{
			DbCommand cmd;
			DbDataReader rdr;
			string sql;
			if(!QSMain.ProjectTables.ContainsKey(Table))
			{
				ErrorString = "Нет описания для таблицы " + Table;
				logger.Error(ErrorString);
				ErrorHappens = true;
				return;
			}
			QSMain.CheckConnectionAlive();
			if(QSMain.ProjectTables[Table].DeleteItems.Count > 0)
			{
				foreach(KeyValuePair<string, TableInfo.DeleteDependenceItem> pair in QSMain.ProjectTables[Table].DeleteItems)
				{
					if(QSMain.ProjectTables[pair.Key].DeleteItems.Count > 0 || QSMain.ProjectTables[pair.Key].ClearItems.Count > 0)
					{
						sql = "SELECT * FROM " +pair.Key + " " + pair.Value.sqlwhere;
						cmd = QSMain.ConnectionDB.CreateCommand();
						cmd.CommandText = sql;
						AddParameters(cmd, pair.Value.SqlParam, ParametersIn);
						rdr = cmd.ExecuteReader();
						List<TableInfo.Params> ReadedData = new List<TableInfo.Params>();
						string IntFieldName = QSMain.ProjectTables[pair.Key].PrimaryKey.ParamInt;
						string StrFieldName = QSMain.ProjectTables[pair.Key].PrimaryKey.ParamStr;
						while(rdr.Read())
						{
							TableInfo.Params OutParam = new TableInfo.Params();
							if(IntFieldName != "")
								OutParam.ParamInt = rdr.GetInt32(rdr.GetOrdinal(IntFieldName));
							if(StrFieldName != "")
								OutParam.ParamStr = rdr.GetString(rdr.GetOrdinal(StrFieldName));
							ReadedData.Add(OutParam);
						}
						rdr.Close ();

						foreach(TableInfo.Params row in ReadedData)
						{
							DeleteObjects (pair.Key, row);
						}
					}

					sql = "DELETE FROM " + pair.Key + " " + pair.Value.sqlwhere;
					cmd = QSMain.ConnectionDB.CreateCommand();
					cmd.CommandText = sql;
					AddParameters(cmd, pair.Value.SqlParam, ParametersIn);
					cmd.ExecuteNonQuery();
				}
			}
			
			if(QSMain.ProjectTables[Table].ClearItems.Count > 0)
			{
				foreach(KeyValuePair<string, TableInfo.ClearDependenceItem> pair in QSMain.ProjectTables[Table].ClearItems)
				{
					sql = "UPDATE " + pair.Key + " SET "; 
					bool first = true;
					foreach (string FieldName in pair.Value.ClearFields)
					{
						if(!first)
							sql += ", ";
						sql += FieldName + " = NULL ";
						first = false;
					}
					sql += pair.Value.sqlwhere;
					cmd = QSMain.ConnectionDB.CreateCommand();
					cmd.CommandText = sql;
					AddParameters(cmd, pair.Value.SqlParam, ParametersIn);
					cmd.ExecuteNonQuery ();
				}
			}

			sql = "DELETE FROM " + Table + " WHERE ";
			bool FirstKey = true;
			if(QSMain.ProjectTables[Table].PrimaryKey.ParamInt != "")
			{
				sql += QSMain.ProjectTables[Table].PrimaryKey.ParamInt + " = @IntParam ";
				FirstKey = false;
			}
			if(QSMain.ProjectTables[Table].PrimaryKey.ParamStr != "")
			{
				if(!FirstKey)
					sql += "AND ";
				sql += QSMain.ProjectTables[Table].PrimaryKey.ParamStr + " = @StrParam ";
				FirstKey = false;
			}
			cmd = QSMain.ConnectionDB.CreateCommand();
			cmd.CommandText = sql;
			TableInfo.PrimaryKeys TempParams = new TableInfo.PrimaryKeys(
				QSMain.ProjectTables[Table].PrimaryKey.ParamInt != "" ? "@IntParam" : "",
				QSMain.ProjectTables[Table].PrimaryKey.ParamStr != "" ? "@StrParam" : ""
			);
			AddParameters(cmd, TempParams, ParametersIn);
			cmd.ExecuteNonQuery();
		}

		void AddParameters(DbCommand cmd, TableInfo.PrimaryKeys SqlParam, TableInfo.Params ParametersIn)
		{
			if(SqlParam.ParamStr != "")
			{
				DbParameter parameterStr = cmd.CreateParameter();
				parameterStr.ParameterName = SqlParam.ParamStr;
				parameterStr.Value = ParametersIn.ParamStr;
				cmd.Parameters.Add(parameterStr);
			}
			if(SqlParam.ParamInt != "")
			{
				DbParameter parameterInt = cmd.CreateParameter();
				parameterInt.ParameterName = SqlParam.ParamInt;
				parameterInt.Value = ParametersIn.ParamInt;
				cmd.Parameters.Add(parameterInt);
			}
		}
	}
}

