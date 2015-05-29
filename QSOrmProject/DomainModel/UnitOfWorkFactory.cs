using System;

namespace QSOrmProject
{
	public static class UnitOfWorkFactory
	{

	/*	public static IUnitOfWork CreateForRoot<TEntity>(TEntity entity) where TEntity : IDomainObject
		{
			var uow = new UnitOfWork();
			return uow;
		} */

		public static IUnitOfWork<TEntity> CreateForRoot<TEntity>(int id) where TEntity : IDomainObject, new()
		{
			var uow = new UnitOfWork<TEntity>(id);
			return uow;
		}

		public static IUnitOfWork<TEntity> CreateWithNewRoot<TEntity>() where TEntity : IDomainObject, new()
		{
			var uow = new UnitOfWork<TEntity>();
			return uow;
		}

	}
}

