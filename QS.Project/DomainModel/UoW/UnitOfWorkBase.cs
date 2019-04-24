using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Event;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.Project.DB;

namespace QS.DomainModel.UoW
{
	public abstract class UnitOfWorkBase : IUnitOfWorkEventHandler
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		//FIXME Временно пока это не будет реализовано напрямую в QS.Project
		public static Action<object[]> NotifyObjectUpdated;

		public event EventHandler<EntityUpdatedEventArgs> SessionScopeEntitySaved;

		protected ITransaction transaction;

		protected ISession session;

		public IHibernateTracker HibernateTracker { get; private set; }

		protected List<object> entityToSave = new List<object>();

		public bool IsNew { get; protected set; }

		public UnitOfWorkTitle ActionTitle { get; protected set; }

		public bool IsAlive {
			get {
				return Session.IsOpen;
			}
		}

		internal virtual ISession InternalSession {
			get {
				if(session == null) {
					session = OrmConfig.OpenSession(null);
					NhEventListener.RegisterUow(this);
					HibernateTracker = TrackerMain.Factory?.CreateHibernateTracker();
				}

				return session;
			}
		}

		public ISession Session => InternalSession;

		public virtual void Commit()
		{
			if(transaction == null || !transaction.IsActive) {
				logger.Warn("Попытка комита закрытой транзацкии.");
				return;
			}

			try {
				transaction.Commit();
				//Записываем журналируемые изменения в новой транзакции, потому как события приходят уже после комита.
				HibernateTracker?.SaveChangeSet((IUnitOfWork)this);

				IsNew = false;
				NotifyObjectUpdated?.Invoke(entityToSave.ToArray());
			} catch(Exception ex) {
				logger.Error(ex, "Исключение в момент комита.");
				if(transaction.IsActive)
					transaction.Rollback();
				throw;
			} finally {
				transaction.Dispose();
				entityToSave.Clear();
				transaction = null;
			}
		}

		protected virtual void DisposeUoW()
		{
			if(transaction != null) {
				if(!transaction.WasCommitted && !transaction.WasRolledBack)
					transaction.Rollback();
				transaction.Dispose();
				transaction = null;
			}
			Session.Dispose();
			NhEventListener.UnregisterUow(this);
		}


		public void Dispose()
		{
			DisposeUoW();
		}

		public IQueryable<T> GetAll<T>() where T : IDomainObject
		{
			return Session.Query<T>();
		}

		public IQueryOver<T, T> Query<T>() where T : class
		{
			return Session.QueryOver<T>();
		}

		public IQueryOver<T, T> Query<T>(Expression<Func<T>> alias) where T : class
		{
			return Session.QueryOver<T>(alias);
		}

		public T GetById<T>(int id) where T : IDomainObject
		{
			return Session.Get<T>(id);
		}

		public IList<T> GetById<T>(IEnumerable<int> ids) where T : class, IDomainObject
		{
			return GetById<T>(ids.ToArray());
		}

		public IList<T> GetById<T>(int[] ids) where T : class, IDomainObject
		{
			return Session.QueryOver<T>()
				.Where(x => x.Id.IsIn(ids)).List();
		}

		public object GetById(Type clazz, int id)
		{
			return Session.Get(clazz, id);
		}

		public virtual void Save<TEntity>(TEntity entity, bool orUpdate = true) where TEntity : IDomainObject
		{
			OpenTransaction();

			if(orUpdate)
				Session.SaveOrUpdate(entity);
			else
				Session.Save(entity);

			RaiseSessionScopeEntitySaved(new object[] { entity });

			if(!entityToSave.Contains(entity))
				entityToSave.Add(entity);
		}

		public virtual void TrySave(object entity, bool orUpdate = true)
		{
			OpenTransaction();

			if(orUpdate)
				Session.SaveOrUpdate(entity);
			else
				Session.Save(entity);

			RaiseSessionScopeEntitySaved(new object[] { entity });

			if(!entityToSave.Contains(entity))
				entityToSave.Add(entity);
		}

		public void Delete<T>(int id) where T : IDomainObject
		{
			Delete(Session.Load<T>(id));
		}

		public void Delete<TEntity>(TEntity entity) where TEntity : IDomainObject
		{
			OpenTransaction();

			Session.Delete(entity);
		}

		public void TryDelete(object entity)
		{
			OpenTransaction();

			Session.Delete(entity);
		}

		private void OpenTransaction()
		{
			if(transaction == null) {
				transaction = Session.BeginTransaction();
			} else if(!transaction.IsActive) {
				transaction.Begin();
			}
		}

		public void RaiseSessionScopeEntitySaved(object[] entity)
		{
			SessionScopeEntitySaved?.Invoke(this, new EntityUpdatedEventArgs(entity));
		}

		#region Обработка событий через IUnitOfWorkEventHandler

		void IUnitOfWorkEventHandler.OnPostLoad(PostLoadEvent loadEvent)
		{

		}

		void IUnitOfWorkEventHandler.OnPreLoad(PreLoadEvent loadEvent)
		{
			if(loadEvent.Entity is IBusinessObject) {
				(loadEvent.Entity as IBusinessObject).UoW = (IUnitOfWork)this;
			}
		}

		void IUnitOfWorkEventHandler.OnPostDelete(PostDeleteEvent deleteEvent)
		{

		}

		#endregion
	}
}

