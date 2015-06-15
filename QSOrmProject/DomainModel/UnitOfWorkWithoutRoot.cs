using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;

namespace QSOrmProject
{
	public class UnitOfWorkWithoutRoot : IUnitOfWork
	{
		private ITransaction transaction;

		private ISession session;

		private List<object> entityToSave = new List<object>();

		public object RootObject {
			get { return null;}
		}

		public bool IsNew { get; private set;}

		public bool HasChanges
		{
			get
			{
				return Session.IsDirty();
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

		public UnitOfWorkWithoutRoot()
		{
			IsNew = false;
		}

		public void Commit()
		{
			try
			{
				transaction.Commit();
				OrmMain.NotifyObjectUpdated(entityToSave.ToArray ());
			}
			catch
			{
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

		public void Save<TEntity>(TEntity entity) where TEntity : IDomainObject
		{
			if(transaction == null)
				transaction = Session.BeginTransaction();

			Session.SaveOrUpdate(entity);

			if (!entityToSave.Contains (entity))
				entityToSave.Add (entity);
		}

		public void Save()
		{
			throw new InvalidOperationException ("В этой реализации UoW отсутствует Root, для завершения транзакции используйте метод Commit()");
		}

		public void Delete<T>(int id) where T : IDomainObject
		{
			Session.Delete(Session.Load<T>(id));
		}

		public void SubcribeOnChanges(Type type)
		{
			var map = OrmMain.GetObjectDiscription (type);
			map.ObjectUpdated+= OnOutObjectUpdated;
		}

		void OnOutObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			
		}
	}
}

