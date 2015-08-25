using System;

namespace QSOrmProject
{
	public interface IUnitOfWork : IDisposable 
	{
		NHibernate.ISession Session{ get;}

		object RootObject{ get;}

		bool IsNew { get;}

		bool IsAlive { get;}

		bool HasChanges{ get;}

		void Save<TEntity>(TEntity entity) where TEntity : IDomainObject;
		void Save();

		T GetById<T>(int id) where T : IDomainObject;

		object GetById(Type clazz, int id);

		void Commit();

		void Delete<TEntity>(TEntity entity) where TEntity : IDomainObject;
	}
}

