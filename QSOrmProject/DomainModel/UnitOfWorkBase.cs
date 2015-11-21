using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;

namespace QSOrmProject
{
	public class UnitOfWorkBase
	{
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
			try
			{
				transaction.Commit();
				IsNew = false;
				OrmMain.NotifyObjectUpdated(entityToSave.ToArray ());
			}
			catch
			{
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
	}
}

