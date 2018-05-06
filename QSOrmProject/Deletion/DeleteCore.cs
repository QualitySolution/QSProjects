using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Gtk;
using QSProjectsLib;
using System.Data;

namespace QSOrmProject.Deletion
{
	public class DeleteCore
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		internal TreeStore ObjectsTreeStore;
		Operation PreparedOperation;
		internal int CountReferenceItems = 0;
		internal List<DeletedItem> DeletedItems = new List<DeletedItem> ();
		internal bool IsHibernateMode;
		internal CheckOperationDlg CheckDlg;
		internal DeleteOperationDlg ExcuteDlg;
		internal System.Action BeforeDeletion;
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

		public DeleteCore(IUnitOfWork uow): this()
		{
			this.uow = uow;
		}

		public DeleteCore()
		{
			ObjectsTreeStore = new TreeStore (typeof(string), typeof(string));
		}

		public bool RunDeletion (string table, int id)
		{
			logger.Debug ("Поиск зависимостей для объекта таблицы {0}...", table);
			var info = DeleteConfig.ClassInfos.FirstOrDefault (i => i.TableName == table);
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
				CheckDlg = new CheckOperationDlg();
				CheckDlg.Show();

				var self = info.GetSelfEntity(this, id);
				PreparedOperation = info.CreateDeleteOperation(self);

				DeletedItems.Add (new DeletedItem {
					ItemClass = info.ObjectClass,
					ItemId = id,
					Title = self.Title
				});

				CountReferenceItems = FillChildOperation (info, PreparedOperation, new TreeIter (), self);
				bool isCanceled = CheckDlg.IsCanceled;
				CheckDlg.Destroy();

				if(isCanceled)
					return false;
			} catch (Exception ex) {
				CheckDlg.Destroy();
				QSMain.ErrorMessageWithLog ("Ошибка в разборе зависимостей удаляемого объекта.", logger, ex);
				return false;
			}

			bool userAccept = DeleteDlg.RunDialog(this);

			if (userAccept) {
				ExcuteDlg = new DeleteOperationDlg();
				ExcuteDlg.SetOperationsCount(PreparedOperation.GetOperationsCount() + 2);
				ExcuteDlg.Show();
				BeforeDeletion?.Invoke();

				try {
					IsHibernateMode = HasHibernateOperations(PreparedOperation);
					PreparedOperation.Execute (this);
					ExcuteDlg.AddExcuteOperation("Операции с журналом изменений");
					DeleteConfig.OnAfterDeletion (sqlTransaction, DeletedItems);
					ExcuteDlg.AddExcuteOperation("Завершение транзакции");
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
				finally
				{
					ExcuteDlg.Destroy();
				}
			}
			return false;
		}

		int FillChildOperation (IDeleteInfo currentDeletion, Operation parentOperation, TreeIter parentIter, EntityDTO masterEntity)
		{
			TreeIter DeleteIter, ClearIter, RemoveIter;
			int Totalcount = 0;
			int DelCount = 0;
			int ClearCount = 0;
			int RemoveCount = 0;
			EntityDTO secondEntity = null;

			var secondInfo = CalculateSecondInfo(currentDeletion, masterEntity);
			if (!currentDeletion.HasDependences && !(secondInfo == null || secondInfo.HasDependences))
				return 0;

			CheckDlg.SetOperationName(String.Format("Проверка ссылок на: {0}", masterEntity.Title));
			logger.Debug(String.Format("Проверка ссылок на: {0}", masterEntity.Title));
			if (CheckDlg.IsCanceled)
				return 0;

			if(secondInfo != null)
			{
				secondEntity = new EntityDTO {
					ClassType = secondInfo.ObjectClass,
					Entity = masterEntity.Entity,
					Id = masterEntity.Id,
					Title = masterEntity.Title
				};
			}

			if (currentDeletion.DeleteItems.Count > 0 || (secondInfo != null && secondInfo.DeleteItems.Count > 0)) {
				if (!ObjectsTreeStore.IterIsValid (parentIter))
					DeleteIter = ObjectsTreeStore.AppendNode ();
				else
					DeleteIter = ObjectsTreeStore.AppendNode (parentIter);

				DelCount = FillDeleteItemsOperation(currentDeletion, parentOperation, DeleteIter, masterEntity, ref Totalcount);

				if(secondInfo != null)
				{
					DelCount += FillDeleteItemsOperation(secondInfo, parentOperation, DeleteIter, secondEntity, ref Totalcount);
				}

				if (DelCount > 0)
					ObjectsTreeStore.SetValues (DeleteIter, String.Format ("Будет удалено ({0}/{1}) объектов:", DelCount, Totalcount));
				else
					ObjectsTreeStore.Remove (ref DeleteIter);
			}

			//TODO Сделать возможность журналирования очистки полей у объектов.
			if (currentDeletion.ClearItems.Count > 0 || (secondInfo != null && secondInfo.ClearItems.Count > 0)) {
				if (!ObjectsTreeStore.IterIsValid (parentIter))
					ClearIter = ObjectsTreeStore.AppendNode ();
				else
					ClearIter = ObjectsTreeStore.AppendNode (parentIter);

				ClearCount = FillCleanItemsOperation(currentDeletion, parentOperation, ClearIter, masterEntity, ref Totalcount);

				if(secondInfo != null)
				{
					ClearCount += FillDeleteItemsOperation(secondInfo, parentOperation, ClearIter, secondEntity, ref Totalcount);
				}
				
				if (ClearCount > 0)
					ObjectsTreeStore.SetValues (ClearIter, RusNumber.FormatCase (ClearCount, 
						"Будет очищено ссылок у {0} объекта:",
						"Будет очищено ссылок у {0} объектов:",
						"Будет очищено ссылок у {0} объектов:"
					));
				else
					ObjectsTreeStore.Remove (ref ClearIter);
			}

			if (currentDeletion.RemoveFromItems.Count > 0 || (secondInfo != null && secondInfo.RemoveFromItems.Count > 0)) {
				if (!ObjectsTreeStore.IterIsValid (parentIter))
					RemoveIter = ObjectsTreeStore.AppendNode ();
				else
					RemoveIter = ObjectsTreeStore.AppendNode (parentIter);

				RemoveCount = FillRemoveFromItemsOperation(currentDeletion, parentOperation, RemoveIter, masterEntity, ref Totalcount);

				if(secondInfo != null)
				{
					RemoveCount += FillRemoveFromItemsOperation(secondInfo, parentOperation, RemoveIter, secondEntity, ref Totalcount);
				}

				if (RemoveCount > 0)
					ObjectsTreeStore.SetValues (RemoveIter, RusNumber.FormatCase (RemoveCount, 
						"Будут очищены ссылки в коллекциях у {0} объекта:",
						"Будут очищены ссылки в коллекциях у {0} объектов:",
						"Будут очищены ссылки в коллекциях у {0} объектов:" ));
				else
					ObjectsTreeStore.Remove (ref RemoveIter);
			}

			return Totalcount;
		}

		private IDeleteInfo CalculateSecondInfo(IDeleteInfo info, EntityDTO entity)
		{
			var hinfo = info as IDeleteInfoHibernate;
			if (hinfo == null)
				return null;

			if(hinfo.IsSubclass)
			{
				var root = hinfo.GetRootDeleteInfo();
				if (root != null)
					return root;
			}

			if(hinfo.IsRootForSubclasses)
			{
				var subclass = NHibernate.NHibernateUtil.GetClass(entity.Entity);

				if (subclass != null)
					return DeleteConfig.GetDeleteInfo(subclass);
			}

			return null;
		}

		private int FillDeleteItemsOperation(IDeleteInfo currentDeletion, Operation parentOperation, TreeIter parentIter, EntityDTO masterEntity, ref int totalCount)
		{
			int deleteCount = 0;
			foreach (var delDepend in currentDeletion.DeleteItems) {
				int GroupCount = 0;
				var childClassInfo = delDepend.GetClassInfo();
				if (childClassInfo == null)
					throw new InvalidOperationException (String.Format ("Зависимость удаления у класса(таблицы) {0}({1}) ссылается на класс(таблицу) {2}({3}) для которого нет описания.", 
						currentDeletion.ObjectClass,
						currentDeletion is DeleteInfo ? (currentDeletion as DeleteInfo).TableName : String.Empty,
						delDepend.ObjectClass, 
						delDepend.TableName));

				var childList = childClassInfo.GetDependEntities(this, delDepend, masterEntity).ToList();

				if (childList.Count == 0)
					continue;

				foreach(var chk in DeletedItems.Where(x => x.ItemClass == childClassInfo.ObjectClass))
				{
					childList.RemoveAll(e => e.Id == chk.ItemId);
				}

				if (childList.Count == 0)
					continue;

				TreeIter GroupIter = ObjectsTreeStore.AppendNode (parentIter);

				var delOper = childClassInfo.CreateDeleteOperation (masterEntity, delDepend, childList);
				if(delDepend.IsCascade)
					parentOperation.ChildAfterOperations.Add (delOper);
				else
					parentOperation.ChildBeforeOperations.Add (delOper);

				foreach (var row in childList) {
					TreeIter ItemIter = ObjectsTreeStore.AppendValues (GroupIter, row.Title);
					DeletedItems.Add (new DeletedItem {
						ItemClass = childClassInfo.ObjectClass,
						ItemId = row.Id,
						Title = row.Title
					});

					totalCount += FillChildOperation (childClassInfo, delOper, ItemIter, row);

					if (CheckDlg.IsCanceled)
						return 0;

					GroupCount++;
					totalCount++;
					deleteCount++;
				}

				CheckDlg.AddLinksCount(GroupCount);

				ObjectsTreeStore.SetValues (GroupIter, String.Format ("{0}({1})", StringWorks.StringToTitleCase (childClassInfo.ObjectsName), GroupCount));
			}
			return deleteCount;
		}

		private int FillCleanItemsOperation(IDeleteInfo currentDeletion, Operation parentOperation, TreeIter parentIter, EntityDTO masterEntity, ref int totalCount)
		{
			int clearCount = 0;
			foreach (var cleanDepend in currentDeletion.ClearItems) {
				int groupCount = 0;
				var childClassInfo = cleanDepend.GetClassInfo();
				if (childClassInfo == null)
					throw new InvalidOperationException (String.Format ("Зависимость очистки у класса {0} ссылается на класс {1} для которого нет описания.", currentDeletion.ObjectClass, cleanDepend.ObjectClass));

				var childList = childClassInfo.GetDependEntities (this, cleanDepend, masterEntity);

				if (childList.Count == 0)
					continue;

				TreeIter GroupIter = ObjectsTreeStore.AppendNode (parentIter);

				var cleanOper = childClassInfo.CreateClearOperation(masterEntity, cleanDepend, childList);

				parentOperation.ChildBeforeOperations.Add (cleanOper);

				foreach(var item in childList)
				{
					ObjectsTreeStore.AppendValues (GroupIter, item.Title);
					groupCount++;
					totalCount++;
					clearCount++;
				}

				CheckDlg.AddLinksCount(groupCount);
				if (CheckDlg.IsCanceled)
					return 0;

				ObjectsTreeStore.SetValues (GroupIter, String.Format ("{0}({1})", StringWorks.StringToTitleCase (childClassInfo.ObjectsName), groupCount));
			}
			return clearCount;
		}

		private int FillRemoveFromItemsOperation(IDeleteInfo currentDeletion, Operation parentOperation, TreeIter parentIter, EntityDTO masterEntity, ref int totalCount)
		{
			int removeCount = 0;
			foreach (var removeDepend in currentDeletion.RemoveFromItems) {
				int groupCount = 0;
				var childClassInfo = removeDepend.GetClassInfo();
				if (childClassInfo == null)
					throw new InvalidOperationException (String.Format ("Зависимость удаления класса {0} ссылается на коллекцию {2} в классе {1} для которого нет описания.", 
						currentDeletion.ObjectClass,
						removeDepend.ObjectClass, 
						removeDepend.CollectionName));

				var childList = childClassInfo.GetDependEntities (this, removeDepend, masterEntity);

				if (childList.Count == 0)
					continue;

				TreeIter GroupIter = ObjectsTreeStore.AppendNode (parentIter);

				var removeOper = childClassInfo.CreateRemoveFromOperation (masterEntity, removeDepend, childList);
				parentOperation.ChildBeforeOperations.Add (removeOper);

				foreach (var row in childList) {
					ObjectsTreeStore.AppendValues (GroupIter, row.Title);
					groupCount++;
					totalCount++;
					removeCount++;
				}

				CheckDlg.AddLinksCount(groupCount);
				if (CheckDlg.IsCanceled)
					return 0;

				var classNames = DomainHelper.GetSubjectNames (childClassInfo.ObjectClass);

				ObjectsTreeStore.SetValues (GroupIter, String.Format ("{2} в {0}({1})", 
					classNames.PrepositionalPlural ?? classNames.NominativePlural,
					groupCount,
					DomainHelper.GetPropertyTitle (removeDepend.ObjectClass, removeDepend.CollectionName)
				));
			}
			return removeCount;
		}

		#region Internal Helpers

		internal static void AddParameterWithId (IDbCommand cmd, uint id)
		{
			var parameterId = cmd.CreateParameter ();
			parameterId.ParameterName = "@id";
			parameterId.Value = id;
			cmd.Parameters.Add (parameterId);
		}

		internal static bool HasHibernateOperations (Operation op)
		{
			return op is IHibernateOperation 
				|| op.ChildBeforeOperations.Any(DeleteCore.HasHibernateOperations)
				|| op.ChildAfterOperations.Any(DeleteCore.HasHibernateOperations);
		}

		internal void ExecuteSql (String sql, uint id)
		{
			logger.Debug ("Выполение SQL={0}", sql);
			if(IsHibernateMode)
			{
				var cmd = UoW.Session.Connection.CreateCommand();
				UoW.Session.Transaction.Enlist(cmd);
				cmd.CommandText = sql;
				DeleteCore.AddParameterWithId (cmd, id);
				cmd.ExecuteNonQuery();
			}
			else
			{
				DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
				cmd.Transaction = SqlTransaction;
				cmd.CommandText = sql;
				DeleteCore.AddParameterWithId (cmd, id);
				cmd.ExecuteNonQuery ();
			}
		}

		#endregion
	}
	 
	public class EntityDTO
	{
		public uint Id;
		public Type ClassType;
		public string Title;
		public object Entity;
	}
}

