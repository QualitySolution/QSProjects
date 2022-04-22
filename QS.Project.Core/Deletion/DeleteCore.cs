using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NHibernate;
using QS.Deletion.Configuration;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.DB;

[assembly:InternalsVisibleTo("QS.Project.Desktop")]
[assembly:InternalsVisibleTo("QS.Project.Gtk")]
[assembly:InternalsVisibleTo("QSOrmProject")]
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

		private bool isOwnerUow;
		IUnitOfWork uow;
		private readonly DbConnection connection;

		IUnitOfWork IDeleteCore.UoW {
			get {
				if (uow == null) {
					uow = UnitOfWorkFactory.CreateWithoutRoot ("Удаление с собственным UnitOfWork");
					isOwnerUow = true;
				}
				return uow;
			}
		}

		DbTransaction sqlTransaction;

		public DbTransaction SqlTransaction {
			get {if(sqlTransaction == null)
					sqlTransaction = connection?.BeginTransaction ();
				return sqlTransaction;
			}
		}

		public DeleteCore(DeleteConfiguration configuration, IUnitOfWork uow = null, DbConnection connection = null)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.uow = uow;
			this.connection = connection;
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

		internal void RemoveItemToClean(EntityDTO entity)
		{
			CleanedItems.Remove(entity);
			OnPropertyChanged(nameof(ItemsToClean));
			OnPropertyChanged(nameof(TotalLinks));
		}

		internal void AddItemToRemoveFrom(EntityDTO entity)
		{
			RemoveFromItems.Add(entity);
			OnPropertyChanged(nameof(ItemsToRemoveFrom));
			OnPropertyChanged(nameof(TotalLinks));
		}

		internal void RemoveItemToRemoveFrom(EntityDTO entity)
		{
			RemoveFromItems.Remove(entity);
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
			else
			{
				//Удаляем все операции очистки ссылок и коллекций для объектов которые сами попали в удаление.
				foreach (var deleted in DeletedItems) {
					bool needRemoveFromOperations = false;
					if(CleanedItems.Contains(deleted)) {
						RemoveItemToClean(deleted);
						needRemoveFromOperations = true;
					}
					if (RemoveFromItems.Contains(deleted)) {
						RemoveItemToRemoveFrom(deleted);
						needRemoveFromOperations = true;
					}
					if (needRemoveFromOperations)
						RemoveDeletedItemsFromOp(RootOperation, deleted, out bool notUse);
				}
			}
		}

		public void RunDeletion(CancellationToken cancellation)
		{
			ProgressUpper = RootOperation.GetOperationsCount() + 2;
			ProgressValue = 0;
			AddExcuteOperation("Подготовка");
			BeforeDeletion?.Invoke();

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
		}

		/// <summary>
		/// Закрывает uow созданный внутри движка.
		/// </summary>
		public void Close()
		{
			if(isOwnerUow)
				uow.Dispose();
		}

		#endregion

		#region Внутренние методы заполнения

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

		#region Обработка операций

		void RemoveDeletedItemsFromOp(Operation operation, EntityDTO entity, out bool isEmptyOp)
		{
			isEmptyOp = false;
			if(operation is HibernateCleanOperation cleanOperation) {
				cleanOperation.ClearingItems.Remove(entity);
				cleanOperation.CleaningEntity.PullsUp.Remove(entity);
				isEmptyOp = cleanOperation.ClearingItems.Count == 0;
			}
			if (operation is HibernateRemoveFromCollectionOperation fromCollectionOperation) {
				fromCollectionOperation.RemoveInItems.Remove(entity);
				fromCollectionOperation.RemovingEntity.PullsUp.Remove(entity);
				isEmptyOp = fromCollectionOperation.RemoveInItems.Count == 0;
			}

			foreach (var childOp in operation.ChildBeforeOperations.ToList()) {
				RemoveDeletedItemsFromOp(childOp, entity, out bool needRemoveOp);
				if (needRemoveOp)
					operation.ChildBeforeOperations.Remove(childOp);
			}

			foreach (var childOp in operation.ChildAfterOperations.ToList()) {
				RemoveDeletedItemsFromOp(childOp, entity, out bool needRemoveOp);
				if (needRemoveOp)
					operation.ChildAfterOperations.Remove(childOp);
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
			logger.Debug ("Выполнение SQL={0}", sql);
			if(IsHibernateMode)
			{
				var cmd = uow.Session.Connection.CreateCommand();
				uow.Session.GetCurrentTransaction().Enlist(cmd);
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