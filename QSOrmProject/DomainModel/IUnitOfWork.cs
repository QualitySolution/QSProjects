using System;

namespace QSOrmProject
{
	public interface IUnitOfWork : IDisposable 
	{
		NHibernate.ISession Session{ get;}

		object RootObject{ get;}

		bool IsNew { get;}

		bool HasChanges{ get;}

		void Save<TEntity>(TEntity entity) where TEntity : IDomainObject;
		void Save();

		T GetById<T>(int id) where T : IDomainObject;

		void Commit();
	}
}

