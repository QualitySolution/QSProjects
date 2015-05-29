using System;

namespace QSOrmProject
{
	public interface IUnitOfWork<TRootEntity> : IDisposable 
		where TRootEntity : IDomainObject, new()
	{
		TRootEntity RootObject { get;}

		bool HasChanges{ get;}

		void Save<TEntity>(TEntity entity) where TEntity : IDomainObject;
		void Save();

		void Commit();
	}
}

