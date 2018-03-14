using System;
using QS.DomainModel;
using QSOrmProject.DomainModel;

namespace QSOrmProject
{
	public static class UnitOfWorkFactory
	{
		/// <summary>
		/// Создаем Unit of Work без корренной сущьности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public static IUnitOfWork CreateWithoutRoot()
		{
			return new UnitOfWorkWithoutRoot();
		}

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id) where TEntity : class, IDomainObject, new()
		{
			var uow = new UnitOfWork<TEntity>(id);
			return uow;
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>() where TEntity : class, IDomainObject, new()
		{
			var uow = new UnitOfWork<TEntity>();
			return uow;
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity) where TEntity : class, IDomainObject, new()
		{
			var uow = new UnitOfWork<TEntity>(entity);
			return uow;
		}

	}
}

