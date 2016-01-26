using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Gtk;
using QSProjectsLib;

namespace QSOrmProject.Deletion
{
	public class DeleteCore
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		internal TreeStore ObjectsTreeStore;
		Operation PreparedOperation;
		internal int CountReferenceItems = 0;
		internal List<DeletedItem> DeletedItems = new List<DeletedItem> ();
		IUnitOfWork uow;

		internal IUnitOfWork UoW{
			get {
				if (uow == null)
					uow = UnitOfWorkFactory.CreateWithoutRoot ();
				return uow;
			}
		}

		DbTransaction sqlTransaction;

		public DbTransaction SqlTransaction {
			get {if(sqlTransaction == null)
					sqlTransaction = QSMain.ConnectionDB.BeginTransaction ();
				return sqlTransaction;
			}
		}

		public DeleteCore()
		{
			ObjectsTreeStore = new TreeStore (typeof(string), typeof(string));
		}

		public bool RunDeletion (string table, int id)
		{
			logger.Debug ("Поиск зависимостей для объекта таблицы {0}...", table);
			var info = DeleteConfig.ClassInfos.OfType<DeleteInfo> ().First (i => i.TableName == table);
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

		private bool Run (IDeleteInfo info, uint id)
		{
			try {
				var self = info.GetSelfEntity(this, id);
				PreparedOperation = info.CreateDeleteOperation(self);

				DeletedItems.Add (new DeletedItem {
					ItemClass = info.ObjectClass,
					ItemId = id,
					Title = self.Title
				});

				CountReferenceItems = FillChildOperation (info, PreparedOperation, new TreeIter (), self);

			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog ("Ошибка в разборе зависимостей удаляемого объекта.", logger, ex);
				return false;
			}

			bool userAccept = DeleteDlg.RunDialog(this);

			if (userAccept) {
				
				try {
					PreparedOperation.Execute (this);
					DeleteConfig.OnAfterDeletion (sqlTransaction, DeletedItems);
					if(sqlTransaction != null)
						sqlTransaction.Commit ();
					if(uow != null)
						uow.Commit ();
					return true;
				} catch (Exception ex) {
					if(SqlTransaction != null)
						sqlTransaction.Rollback ();
					QSMain.ErrorMessageWithLog ("Ошибка при удалении", logger, ex);
				}
			}
			return false;
		}

		int FillChildOperation (IDeleteInfo currentDeletion, Operation parentOperation, TreeIter parentIter, EntityDTO masterEntity)
		{
			TreeIter DeleteIter, ClearIter, RemoveIter, GroupIter, ItemIter;
			int Totalcount = 0;
			int DelCount = 0;
			int ClearCount = 0;
			int RemoveCount = 0;
			int GroupCount;

			if (currentDeletion.DeleteItems.Count > 0) {
				if (!ObjectsTreeStore.IterIsValid (parentIter))
					DeleteIter = ObjectsTreeStore.AppendNode ();
				else
					DeleteIter = ObjectsTreeStore.AppendNode (parentIter);
				foreach (var delDepend in currentDeletion.DeleteItems) {
					GroupCount = 0;
					var childClassInfo = delDepend.GetClassInfo();
					if (childClassInfo == null)
						throw new InvalidOperationException (String.Format ("Зависимость удаления у класса(таблицы) {0}({1}) ссылается на класс(таблицу) {2}({3}) для которого нет описания.", 
							currentDeletion.ObjectClass,
							currentDeletion is DeleteInfo ? (currentDeletion as DeleteInfo).TableName : String.Empty,
							delDepend.ObjectClass, 
							delDepend.TableName));

					var childList = childClassInfo.GetDependEntities (this, delDepend, masterEntity);

					if (childList.Count == 0)
						continue;

					GroupIter = ObjectsTreeStore.AppendNode (DeleteIter);

					var delOper = childClassInfo.CreateDeleteOperation (masterEntity, delDepend, childList);
					parentOperation.ChildOperations.Add (delOper);

					foreach (var row in childList) {
						ItemIter = ObjectsTreeStore.AppendValues (GroupIter, row.Title);
						DeletedItems.Add (new DeletedItem {
							ItemClass = childClassInfo.ObjectClass,
							ItemId = row.Id,
							Title = row.Title
						});
						if (childClassInfo.DeleteItems.Count > 0 || childClassInfo.ClearItems.Count > 0) {
							Totalcount += FillChildOperation (childClassInfo, delOper, ItemIter, row);
						}
						GroupCount++;
						Totalcount++;
						DelCount++;
					}

					ObjectsTreeStore.SetValues (GroupIter, String.Format ("{0}({1})", StringWorks.StringToTitleCase (childClassInfo.ObjectsName), GroupCount));
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
				foreach (var cleanDepend in currentDeletion.ClearItems) {
					GroupCount = 0;
					var childClassInfo = cleanDepend.GetClassInfo();
					if (childClassInfo == null)
						throw new InvalidOperationException (String.Format ("Зависимость очистки у класса {0} ссылается на класс {1} для которого нет описания.", currentDeletion.ObjectClass, cleanDepend.ObjectClass));

					var childList = childClassInfo.GetDependEntities (this, cleanDepend, masterEntity);

					if (childList.Count == 0)
						continue;

					GroupIter = ObjectsTreeStore.AppendNode (ClearIter);

					var cleanOper = childClassInfo.CreateClearOperation(masterEntity, cleanDepend, childList);

					parentOperation.ChildOperations.Add (cleanOper);

					foreach(var item in childList)
					{
						ItemIter = ObjectsTreeStore.AppendValues (GroupIter, item.Title);
						GroupCount++;
						Totalcount++;
						ClearCount++;
					}
					ObjectsTreeStore.SetValues (GroupIter, String.Format ("{0}({1})", StringWorks.StringToTitleCase (childClassInfo.ObjectsName), GroupCount));
				}
				if (ClearCount > 0)
					ObjectsTreeStore.SetValues (ClearIter, String.Format ("Будет очищено ссылок у {0} объектов:", ClearCount));
				else
					ObjectsTreeStore.Remove (ref ClearIter);
			}

			if (currentDeletion.RemoveFromItems.Count > 0) {
				if (!ObjectsTreeStore.IterIsValid (parentIter))
					RemoveIter = ObjectsTreeStore.AppendNode ();
				else
					RemoveIter = ObjectsTreeStore.AppendNode (parentIter);
				foreach (var delDepend in currentDeletion.RemoveFromItems) {
					GroupCount = 0;
					var childClassInfo = delDepend.GetClassInfo();
					if (childClassInfo == null)
						throw new InvalidOperationException (String.Format ("Зависимость удаления класса {0} ссылается на коллекцию {2} в классе {1} для которого нет описания.", 
							currentDeletion.ObjectClass,
							delDepend.ObjectClass, 
							delDepend.CollectionName));

					var childList = childClassInfo.GetDependEntities (this, delDepend, masterEntity);

					if (childList.Count == 0)
						continue;

					GroupIter = ObjectsTreeStore.AppendNode (RemoveIter);

					var removeOper = childClassInfo.CreateRemoveFromOperation (masterEntity, delDepend, childList);
					parentOperation.ChildOperations.Add (removeOper);

					foreach (var row in childList) {
						ItemIter = ObjectsTreeStore.AppendValues (GroupIter, row.Title);
						GroupCount++;
						Totalcount++;
						RemoveCount++;
					}

					ObjectsTreeStore.SetValues (GroupIter, String.Format ("{0}({1})", StringWorks.StringToTitleCase (childClassInfo.ObjectsName), GroupCount));
				}
				if (RemoveCount > 0)
					ObjectsTreeStore.SetValues (RemoveIter, String.Format ("Будут очищены ссылки в коллекциях у {0} объектов:", RemoveCount));
				else
					ObjectsTreeStore.Remove (ref RemoveIter);
			}

			return Totalcount;
		}

		internal static void AddParameterWithId (DbCommand cmd, uint id)
		{
			DbParameter parameterId = cmd.CreateParameter ();
			parameterId.ParameterName = "@id";
			parameterId.Value = id;
			cmd.Parameters.Add (parameterId);
		}
	}

	public class EntityDTO
	{
		public uint Id;
		public string Title;
		public object Entity;
	}
}

