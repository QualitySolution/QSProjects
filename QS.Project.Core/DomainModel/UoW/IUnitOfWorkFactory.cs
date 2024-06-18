using System;
using System.Runtime.CompilerServices;
using QS.DomainModel.Entity;

namespace QS.DomainModel.UoW
{
	public interface IUnitOfWorkFactory
	{
		/// <summary>
		/// Создаем Unit of Work без конкретной сущности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0);

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		[Obsolete("IUnitOfWorkGeneric будет удален в будущем, используйте обычный IUnitOfWork")]
		IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new();

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		[Obsolete("IUnitOfWorkGeneric будет удален в будущем, используйте обычный IUnitOfWork")]
		IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new();

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		[Obsolete("IUnitOfWorkGeneric будет удален в будущем, используйте обычный IUnitOfWork")]
		IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new();
	}
}
