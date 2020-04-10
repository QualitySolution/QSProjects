using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using QS.Deletion.Configuration;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.DB;

namespace QS.Deletion
{
	public class DeleteCore : PropertyChangedBase, IDeleteCore
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private readonly DeleteConfiguration configuration;
		internal Operation RootOperation;
		internal EntityDTO RootEntity;
		internal bool IsHibernateMode;
		internal System.Action BeforeDeletion;

		IUnitOfWork uow;

		IUnitOfWork IDeleteCore.UoW {
			get {
				if (uow == null)
					uow = UnitOfWorkFactory.CreateWithoutRoot ("Удаление с собственным UnitOfWork");
				return uow;
			}
		}

		DbTransaction sqlTransaction;

		public DbTransaction SqlTransaction {
			get {if(sqlTransaction == null)
					sqlTransaction = Connection.ConnectionDB.BeginTransaction ();
				return sqlTransaction;
			}
		}

		public DeleteCore(DeleteConfiguration configuration, IUnitOfWork uow = null)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.uow = uow;
		}

		#region Свойства описывающие текущее состояние

		// 1 это базовое удаление оно не считается ссылкой.
		public uint TotalLinks => (uint)DeletedItems.Count + (uint)CleanedItems.Count + (uint)RemoveFromItems.Count - 1;

		public uint ItemsToDelete => (uint)DeletedItems.Count;
		public uint ItemsToClean => (uint)CleanedItems.Count;
		public uint ItemsToRemoveFrom => (uint)RemoveFromItems.Count;

		private string operationTitle;
		public virtual string OperationTitle {
			get => operationTitle;
			set => SetField(ref operationTitle, value);
		}

		private double progressValue;
		public virtual double ProgressValue {
			get => progressValue;
			set => SetField(ref progressValue, value);
		}

		private double progressUpper;
		public virtual double ProgressUpper {
			get => progressUpper;
			set => SetField(ref progressUpper, value);
		}

		private bool? deletionExecuted;
		public virtual bool? DeletionExecuted {
			get => deletionExecuted;
			set => SetField(ref deletionExecuted, value);
		}

		#endregion

		#region Работа с коллекциями
		internal List<EntityDTO> DeletedItems = new List<EntityDTO>();
		internal List<EntityDTO> CleanedItems = new List<EntityDTO>();
		internal List<EntityDTO> RemoveFromItems = new List<EntityDTO>();

		internal void AddItemToDelete(EntityDTO entity)
		{
			DeletedItems.Add(entity);
			OnPropertyChanged(nameof(ItemsToDelete));
			OnPropertyChanged(nameof(TotalLinks));
		}

		internal void AddItemToClean(EntityDTO entity)
		{
			CleanedItems.Add(entity);
			OnPropertyChanged(nameof(ItemsToClean));
			OnPropertyChanged(nameof(TotalLinks));
		}

		internal void AddItemToRemoveFrom(EntityDTO entity)
		{
			RemoveFromItems.Add(entity);
			OnPropertyChanged(nameof(ItemsToRemoveFrom));
			OnPropertyChanged(nameof(TotalLinks));
		}

		#endregion

		#region Внешние команды

		public void PrepareDeletion (Type objectClass, int id, CancellationToken cancellation)
		{
			logger.Debug ("Поиск зависимостей для класса {0}...", objectClass);
			var info = configuration.ClassInfos.Find (i => i.ObjectClass == objectClass);
			if (info == null)
				throw new InvalidOperationException (String.Format ("Удаление для класса {0} не настроено в DeleteConfig", objectClass));
				
			RootEntity = info.GetSelfEntity(this, (uint)id);
			RootOperation = info.CreateDeleteOperation(RootEntity);

			AddItemToDelete(RootEntity);

			FillChildOperation(info, RootOperation, RootEntity, cancellation);

			if (cancellation.IsCancellationRequested)
				DeletionExecuted = false;
		}

		public void RunDeletion(CancellationToken cancellation)
		{
			ProgressUpper = RootOperation.GetOperationsCount() + 2;
			ProgressValue = 0;
			AddExcuteOperation("Подготовка");
			BeforeDeletion?.Invoke();

			try {
				IsHibernateMode = HasHibernateOperations(RootOperation);
				RootOperation.Execute (this, cancellation);

				if(cancellation.IsCancellationRequested) {
					DeletionExecuted = false;
					return;
				}

				AddExcuteOperation("Завершение транзакции");
				if(sqlTransaction != null)
					sqlTransaction.Commit ();
				if(uow != null)
					uow.Commit ();
				DeletionExecuted = true;
				return;
			} catch (Exception ex) {
				DeletionExecuted = false;
				if(SqlTransaction != null)
					sqlTransaction.Rollback ();
				throw ex;
			}
		}

		#endregion

		void FillChildOperation (IDeleteInfo currentDeletion, Operation parentOperation, EntityDTO masterEntity, CancellationToken cancellation)
		{
			EntityDTO secondEntity = null;

			var secondInfo = CalculateSecondInfo(currentDeletion, masterEntity);
			if (!currentDeletion.HasDependences && !(secondInfo == null || secondInfo.HasDependences))
				return;

			OperationTitle = String.Format("Проверка ссылок на: {0}", masterEntity.Title);
			logger.Info(OperationTitle);

			if (cancellation.IsCancellationRequested)
				return;

			if (secondInfo != null)
			{
				secondEntity = new EntityDTO {
					ClassType = secondInfo.ObjectClass,
					Entity = masterEntity.Entity,
					Id = masterEntity.Id,
					Title = masterEntity.Title
				};
			}

			if (currentDeletion.DeleteItems.Count > 0) {
				FillDeleteItemsOperation(currentDeletion, parentOperation, masterEntity, cancellation);
			}

			if (cancellation.IsCancellationRequested)
				return;

			if (secondInfo != null && secondInfo.DeleteItems.Count > 0) {
				FillDeleteItemsOperation(secondInfo, parentOperation, secondEntity, cancellation);
			}

			if (cancellation.IsCancellationRequested)
				return;

			if (currentDeletion.ClearItems.Count > 0 ) 
				FillCleanItemsOperation(currentDeletion, parentOperation, masterEntity, cancellation);

			if (cancellation.IsCancellationRequested)
				return;

			if (secondInfo != null && secondInfo.ClearItems.Count > 0)
				FillCleanItemsOperation(secondInfo, parentOperation, secondEntity, cancellation);

			if (cancellation.IsCancellationRequested)
				return;

			if (currentDeletion.RemoveFromItems.Count > 0) 
				FillRemoveFromItemsOperation(currentDeletion, parentOperation, masterEntity, cancellation);

			if (cancellation.IsCancellationRequested)
				return;

			if (secondInfo != null && secondInfo.RemoveFromItems.Count > 0)
				FillRemoveFromItemsOperation(secondInfo, parentOperation, secondEntity, cancellation);

			if(secondEntity != null)
				masterEntity.PullsUp.AddRange(secondEntity.PullsUp);
		}

		#region Внутренние методы заполнения

		private IDeleteInfo CalculateSecondInfo(IDeleteInfo info, EntityDTO entity)
		{
			var hinfo = info as IDeleteInfoHibernate;
			if (hinfo == null)
				return null;

			if(hinfo.IsSubclass)
			{
				var root = hinfo.GetRootClass();
				if (root != null)
					return configuration.GetDeleteInfo(root);
			}

			if(hinfo.IsRootForSubclasses)
			{
				var subclass = NHibernate.NHibernateUtil.GetClass(entity.Entity);

				if (subclass != null)
					return configuration.GetDeleteInfo(subclass);
			}

			return null;
		}

		private void FillDeleteItemsOperation(IDeleteInfo currentDeletion, Operation parentOperation, EntityDTO masterEntity, CancellationToken cancellation)
		{
			foreach (var delDepend in currentDeletion.DeleteItems) {
				var childClassInfo = configuration.GetDeleteInfo(delDepend);
				if (childClassInfo == null)
					throw new InvalidOperationException (String.Format ("Зависимость удаления у класса(таблицы) {0}({1}) ссылается на класс(таблицу) {2}({3}) для которого нет описания.", 
						currentDeletion.ObjectClass,
						currentDeletion is DeleteInfo ? (currentDeletion as DeleteInfo).TableName : String.Empty,
						delDepend.ObjectClass, 
						delDepend.TableName));

				var childList = childClassInfo.GetDependEntities(this, delDepend, masterEntity).ToList();

				if (childList.Count == 0)
					continue;

				foreach(var chk in DeletedItems.Where(x => x.ClassType == childClassInfo.ObjectClass))
				{
					childList.RemoveAll(e => e.Id == chk.Id);
				}

				if (childList.Count == 0)
					continue;

				var delOper = childClassInfo.CreateDeleteOperation (masterEntity, delDepend, childList);
				if(delDepend.IsCascade)
					parentOperation.ChildAfterOperations.Add (delOper);
				else
					parentOperation.ChildBeforeOperations.Add (delOper);

				foreach (var row in childList) {
					AddItemToDelete (row);
					masterEntity.PullsUp.Add(row);

					FillChildOperation (childClassInfo, delOper, row, cancellation);

					if (cancellation.IsCancellationRequested)
						return;
				}
			}
		}

		private void FillCleanItemsOperation(IDeleteInfo currentDeletion, Operation parentOperation, EntityDTO masterEntity, CancellationToken cancellation)
		{
			foreach (var cleanDepend in currentDeletion.ClearItems) {
				var childClassInfo = configuration.GetDeleteInfo(cleanDepend);
				if (childClassInfo == null)
					throw new InvalidOperationException (String.Format ("Зависимость очистки у класса {0} ссылается на класс {1} для которого нет описания.", currentDeletion.ObjectClass, cleanDepend.ObjectClass));

				var childList = childClassInfo.GetDependEntities (this, cleanDepend, masterEntity);

				if (childList.Count == 0)
					continue;
					
				var cleanOper = childClassInfo.CreateClearOperation(masterEntity, cleanDepend, childList);

				parentOperation.ChildBeforeOperations.Add (cleanOper);

				foreach(var item in childList)
				{
					AddItemToClean(item);
					masterEntity.PullsUp.Add(item);
				}
			}
		}

		private void FillRemoveFromItemsOperation(IDeleteInfo currentDeletion, Operation parentOperation, EntityDTO masterEntity, CancellationToken cancellation)
		{
			foreach (var removeDepend in currentDeletion.RemoveFromItems) {
				var childClassInfo = configuration.GetDeleteInfo(removeDepend);
				if (childClassInfo == null)
					throw new InvalidOperationException (String.Format ("Зависимость удаления класса {0} ссылается на коллекцию {2} в классе {1} для которого нет описания.", 
						currentDeletion.ObjectClass,
						removeDepend.ObjectClass, 
						removeDepend.CollectionName));

				var childList = childClassInfo.GetDependEntities (this, removeDepend, masterEntity);

				if (childList.Count == 0)
					continue;

				var removeOper = childClassInfo.CreateRemoveFromOperation (masterEntity, removeDepend, childList);
				parentOperation.ChildBeforeOperations.Add (removeOper);

				foreach (var row in childList) {
					AddItemToRemoveFrom(row);
					masterEntity.PullsUp.Add(row);
				}
			}
		}

		#endregion

		#region UI

		internal void AddExcuteOperation(string text)
		{
			OperationTitle = text;
			ProgressValue++;
		}

		void IDeleteCore.AddExcuteOperation(string text)
		{
			AddExcuteOperation(text);
		}

		#endregion

		#region Internal Helpers

		internal static bool HasHibernateOperations (Operation op)
		{
			return op is IHibernateOperation 
				|| op.ChildBeforeOperations.Any(DeleteCore.HasHibernateOperations)
				|| op.ChildAfterOperations.Any(DeleteCore.HasHibernateOperations);
		}

		void IDeleteCore.ExecuteSql (String sql, uint id)
		{
			logger.Debug ("Выполение SQL={0}", sql);
			if(IsHibernateMode)
			{
				var cmd = uow.Session.Connection.CreateCommand();
				uow.Session.Transaction.Enlist(cmd);
				cmd.CommandText = sql;
				InternalHelper.AddParameterWithId (cmd, id);
				cmd.ExecuteNonQuery();
			}
			else
			{
				DbCommand cmd = SqlTransaction.Connection.CreateCommand ();
				cmd.CommandText = sql;
				InternalHelper.AddParameterWithId (cmd, id);
				cmd.ExecuteNonQuery ();
			}
		}

		#endregion
	}
}

