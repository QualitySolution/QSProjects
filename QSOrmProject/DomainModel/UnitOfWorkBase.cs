using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace QSOrmProject
{
	public class UnitOfWorkBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		protected ITransaction transaction;

		protected ISession session;

		protected List<object> entityToSave = new List<object>();

		public bool IsNew { get; protected set;}

		public bool IsAlive { get{ return Session.IsOpen;
			}}

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.OpenSession ();
				return session;
			}
			protected set { session = value; }
		}

		public UnitOfWorkBase ()
		{
		}

		public void Commit()
		{
			if (transaction == null)
			{
				logger.Warn ("Попытка комита закрытой транзацкии.");
				return;
			}

			try
			{
				transaction.Commit();
				IsNew = false;
				OrmMain.NotifyObjectUpdated(entityToSave.ToArray ());
			}
			catch(Exception ex)
			{
				logger.Error(ex, "Исключение в момент комита.");
				if(transaction.IsActive)
					transaction.Rollback();
				throw;
			}
			finally
			{
				transaction.Dispose();
				entityToSave.Clear ();
				transaction = null;
			}
		}

		public void Dispose()
		{
			if (transaction != null)
			{
				if (!transaction.WasCommitted && !transaction.WasRolledBack)
					transaction.Rollback();
				transaction.Dispose();
				transaction = null;
			}
				
			Session.Dispose();
		}

		public IQueryable<T> GetAll<T>() where T : IDomainObject
		{
			return Session.Query<T>();
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

		public virtual void Save<TEntity>(TEntity entity) where TEntity : IDomainObject
		{
			if(transaction == null)
				transaction = Session.BeginTransaction();

			Session.SaveOrUpdate(entity);

			if (!entityToSave.Contains (entity))
				entityToSave.Add (entity);
		}

		public virtual void TrySave(object entity)
		{
			if(transaction == null)
				transaction = Session.BeginTransaction();

			Session.SaveOrUpdate(entity);

			if (!entityToSave.Contains (entity))
				entityToSave.Add (entity);
		}

		public void Delete<T>(int id) where T : IDomainObject
		{
			Delete(Session.Load<T>(id));
		}

		public void Delete<TEntity>(TEntity entity) where TEntity : IDomainObject
		{
			if(transaction == null)
				transaction = Session.BeginTransaction();

			Session.Delete (entity);
		}

		public void TryDelete(object entity)
		{
			if(transaction == null)
				transaction = Session.BeginTransaction();

			Session.Delete (entity);
		}
	}
}

