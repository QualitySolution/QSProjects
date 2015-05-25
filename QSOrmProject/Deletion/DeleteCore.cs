using System;
using QSProjectsLib;
using System.Data.Common;
using System.Collections.Generic;
using Gtk;

namespace QSOrmProject.Deletion
{
	public class DeleteCore
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		internal TreeStore ObjectsTreeStore;
		DeleteOperation PreparedOperation;
		internal int CountReferenceItems = 0;
		internal List<DeletedItem> DeletedItems = new List<DeletedItem> ();

		public DeleteCore()
		{
			ObjectsTreeStore = new TreeStore (typeof(string), typeof(string));
		}

		public bool RunDeletion (string table, int id)
		{
			logger.Debug ("Поиск зависимостей для объекта таблицы {0}...", table);
			var info = DeleteConfig.ClassInfos.Find (i => i.TableName == table);
			if (info == null)
				throw new InvalidOperationException (String.Format ("Удаление для объектов таблицы {0} не настроено в DeleteConfig", table));

			return Run (info, Convert.ToUInt32 (id));
		}

		public bool RunDeletion (Type objectClass, int id)
		{
			logger.Debug ("Поиск зависимостей для класса {0}...", objectClass);
			var info = DeleteConfig.ClassInfos.Find (i => i.ObjectClass == objectClass);
			if (info == null)
				throw new InvalidOperationException (String.Format ("Удаление для класса {0} не настроено в DeleteConfig", objectClass));

			return Run (info, Convert.ToUInt32 (id));
		}

		private bool Run (DeleteInfo info, uint id)
		{
			try {
				PreparedOperation = new DeleteOperation () {
					ItemId = id,
					TableName = info.TableName,
					WhereStatment = "WHERE id = @id"
				};

				var cmd = QSMain.ConnectionDB.CreateCommand ();
				cmd.CommandText = info.PreparedSqlSelect + 
					String.Format ("WHERE {0}.id = @id", info.TableName);
				AddParameterWithId (cmd, id);

				using (DbDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read ();
					int IndexOfIdParam = rdr.GetOrdinal ("id");
					object[] Fields = new object[rdr.FieldCount];
					rdr.GetValues (Fields);

					DeletedItems.Add (new DeletedItem {
						ItemClass = info.ObjectClass,
						ItemId = id,
						Title = String.Format (info.DisplayString, Fields)
					});
				}

				CountReferenceItems = FillChildOperation (info, PreparedOperation, new TreeIter (), Convert.ToUInt32 (id));
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog ("Ошибка в разборе зависимостей удаляемого объекта.", logger, ex);
				return false;
			}

			bool userAccept = DeleteDlg.RunDialog(this);

			if (userAccept) {
				var trans = QSMain.ConnectionDB.BeginTransaction ();
				try {
					PreparedOperation.Execute (trans);
					DeleteConfig.OnAfterDeletion (trans, DeletedItems);
					trans.Commit ();
					return true;
				} catch (Exception ex) {
					trans.Rollback ();
					QSMain.ErrorMessageWithLog ("Ошибка при удалении", logger, ex);
				}
			}
			return false;
		}

		int FillChildOperation (DeleteInfo currentDeletion, Operation parentOperation, TreeIter parentIter, uint currentId)
		{
			TreeIter DeleteIter, ClearIter, GroupIter, ItemIter;
			int Totalcount = 0;
			int DelCount = 0;
			int ClearCount = 0;
			int GroupCount;
			DbCommand cmd;
			QSMain.CheckConnectionAlive ();

			if (currentDeletion.DeleteItems.Count > 0) {
				if (!ObjectsTreeStore.IterIsValid (parentIter))
					DeleteIter = ObjectsTreeStore.AppendNode ();
				else
					DeleteIter = ObjectsTreeStore.AppendNode (parentIter);
				foreach (var delItem in currentDeletion.DeleteItems) {
					GroupCount = 0;
					var childClassInfo = delItem.GetClassInfo ();
					if (childClassInfo == null)
						throw new InvalidOperationException (String.Format ("Зависимость удаления у класса(таблицы) {0}({1}) ссылается на класс(таблицу) {2}({3}) для которого нет описания.", 
							currentDeletion.ObjectClass, currentDeletion.TableName, delItem.ObjectClass, delItem.TableName));

					string sql = childClassInfo.PreparedSqlSelect + delItem.WhereStatment;
					cmd = QSMain.ConnectionDB.CreateCommand ();
					cmd.CommandText = sql;
					AddParameterWithId (cmd, currentId);

					List<object[]> ReadedData = new List<object[]> ();
					int IndexOfIdParam;

					using (DbDataReader rdr = cmd.ExecuteReader ()) {
						if (!rdr.HasRows) {
							continue;
						}
						GroupIter = ObjectsTreeStore.AppendNode (DeleteIter);
						IndexOfIdParam = rdr.GetOrdinal ("id");
						while (rdr.Read ()) {
							object[] Fields = new object[rdr.FieldCount];
							rdr.GetValues (Fields);
							ReadedData.Add (Fields);
						}
					}

					if (ReadedData.Count > 0) {
						var delOper = new DeleteOperation () {
							ItemId = currentId,
							TableName = childClassInfo.TableName,
							WhereStatment = delItem.WhereStatment
						};
						parentOperation.ChildOperations.Add (delOper);

						foreach (object[] row in ReadedData) {
							ItemIter = ObjectsTreeStore.AppendValues (GroupIter, String.Format (childClassInfo.DisplayString, row));
							DeletedItems.Add (new DeletedItem {
								ItemClass = childClassInfo.ObjectClass,
								ItemId = (uint)row [IndexOfIdParam],
								Title = String.Format (childClassInfo.DisplayString, row)
							});
							if (childClassInfo.DeleteItems.Count > 0 || childClassInfo.ClearItems.Count > 0) {
								Totalcount += FillChildOperation (childClassInfo, delOper, ItemIter, (uint)row [IndexOfIdParam]);
							}
							GroupCount++;
							Totalcount++;
							DelCount++;
						}
					}

					ObjectsTreeStore.SetValues (GroupIter, String.Format ("{0}({1})", childClassInfo.ObjectsName, GroupCount));
				}
				if (DelCount > 0)
					ObjectsTreeStore.SetValues (DeleteIter, String.Format ("Будет удалено ({0}/{1}) объектов:", DelCount, Totalcount));
				else
					ObjectsTreeStore.Remove (ref DeleteIter);
			}

			//TODO Сделать возможность журналирования очистки полей у объектов.
			if (currentDeletion.ClearItems.Count > 0) {
				if (!ObjectsTreeStore.IterIsValid (parentIter))
					ClearIter = ObjectsTreeStore.AppendNode ();
				else
					ClearIter = ObjectsTreeStore.AppendNode (parentIter);
				foreach (var cleanItem in currentDeletion.ClearItems) {
					GroupCount = 0;
					var childClassInfo = cleanItem.GetClassInfo ();
					if (childClassInfo == null)
						throw new InvalidOperationException (String.Format ("Зависимость очистки у класса {0} ссылается на класс {1} для которого нет описания.", currentDeletion.ObjectClass, cleanItem.ObjectClass));

					string sql = childClassInfo.PreparedSqlSelect + cleanItem.WhereStatment;
					cmd = QSMain.ConnectionDB.CreateCommand ();
					cmd.CommandText = sql;
					AddParameterWithId (cmd, currentId);

					using (DbDataReader rdr = cmd.ExecuteReader ()) {
						if (!rdr.HasRows) {
							rdr.Close ();
							continue;
						}
						GroupIter = ObjectsTreeStore.AppendNode (ClearIter);

						var cleanOper = new CleanOperation () {
							ItemId = currentId,
							TableName = childClassInfo.TableName,
							CleanFields = cleanItem.ClearFields,
							WhereStatment = cleanItem.WhereStatment
						};
						parentOperation.ChildOperations.Add (cleanOper);

						while (rdr.Read ()) {
							object[] Fields = new object[rdr.FieldCount];
							rdr.GetValues (Fields);
							ItemIter = ObjectsTreeStore.AppendValues (GroupIter, String.Format (childClassInfo.DisplayString, Fields));
							GroupCount++;
							Totalcount++;
							ClearCount++;
						}
						ObjectsTreeStore.SetValues (GroupIter, String.Format ("{0}({1})", childClassInfo.ObjectsName, GroupCount));
					}
				}
				if (ClearCount > 0)
					ObjectsTreeStore.SetValues (ClearIter, String.Format ("Будет очищено ссылок у {0} объектов:", ClearCount));
				else
					ObjectsTreeStore.Remove (ref ClearIter);
			}
			return Totalcount;
		}

		static void AddParameterWithId (DbCommand cmd, uint id)
		{
			DbParameter parameterId = cmd.CreateParameter ();
			parameterId.ParameterName = "@id";
			parameterId.Value = id;
			cmd.Parameters.Add (parameterId);
		}

		abstract class Operation
		{
			public uint ItemId;
			public List<Operation> ChildOperations = new List<Operation> ();

			public abstract void Execute (DbTransaction trans);
		}

		class DeleteOperation : Operation
		{
			public string TableName;
			public string WhereStatment;

			public override void Execute (DbTransaction trans)
			{
				ChildOperations.ForEach (o => o.Execute (trans));

				DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
				cmd.Transaction = trans;
				cmd.CommandText = String.Format ("DELETE FROM {0} {1}", TableName, WhereStatment);
				AddParameterWithId (cmd, ItemId);
				cmd.ExecuteNonQuery ();
			}
		}

		class CleanOperation : Operation
		{
			public string TableName;
			public string WhereStatment;
			public string[] CleanFields;

			public override void Execute (DbTransaction trans)
			{
				var sql = new DBWorks.SQLHelper ("UPDATE {0} SET ", TableName);
				sql.StartNewList ("", ", ");
				foreach (string FieldName in CleanFields) {
					sql.AddAsList ("{0} = NULL ", args: FieldName);
				}
				sql.Add (WhereStatment);

				DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
				cmd.Transaction = trans;
				cmd.CommandText = sql.Text;
				AddParameterWithId (cmd, ItemId);
				cmd.ExecuteNonQuery ();
			}
		}
	}
}

