using System;
using NHibernate;
using System.Linq;
using NHibernate.Linq;

namespace QSOrmProject
{
	public class UnitOfWork<TRootEntity> : IUnitOfWorkGeneric<TRootEntity> where TRootEntity : IDomainObject, new()
	{
		private ITransaction transaction;

		private ISession session;

		public object RootObject {
			get { return Root;}
		}

		public TRootEntity Root { get; private set;}

		public bool IsNew { get; private set;}

		public bool HasChanges
		{
			get
			{
				return IsNew || Session.IsDirty();
			}
		}

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		public UnitOfWork()
		{
			IsNew = true;
			Root = new TRootEntity();
		}

		public UnitOfWork(int id)
		{
			IsNew = false;
			Root = GetById<TRootEntity>(id);
		}

		public void Commit()
		{
			try
			{
				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
			finally
			{
				transaction.Dispose();
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

		public void Save<TEntity>(TEntity entity) where TEntity : IDomainObject
		{
			if(transaction == null)
				transaction = Session.BeginTransaction();

			Session.SaveOrUpdate(entity);

			if (RootObject.Equals(entity))
			{
				Commit();
				OrmMain.NotifyObjectUpdated(RootObject);
			}
			else
			{
				OrmMain.DelayedNotifyObjectUpdated(RootObject, entity);
			}
		}

		public void Save()
		{
			Save(Root);
		}

		public void Delete<T>(int id) where T : IDomainObject
		{
			Session.Delete(Session.Load<T>(id));
		}
	}
}

