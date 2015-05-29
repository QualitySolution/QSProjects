using System;

namespace QSOrmProject
{
	public interface IUnitOfWork : IDisposable 
	{
		object RootObject{ get;}

		bool HasChanges{ get;}

		void Save<TEntity>(TEntity entity) where TEntity : IDomainObject;
		void Save();

		void Commit();
	}
}

