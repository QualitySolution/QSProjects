using System.Runtime.CompilerServices;
using QS.DomainModel.Entity;

namespace QS.DomainModel.UoW
{
	public interface IUnitOfWorkFactory
	{
		/// <summary>
		/// Создаем Unit of Work без корренной сущьности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0);

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new();

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new();

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new();

		/// <summary>
		/// Создаем дочерний Unit of Work не использующий коммит при сохранении Root 
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TChildRootEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		IUnitOfWorkGeneric<TChildRootEntity> CreateForChildRoot<TChildRootEntity>(TChildRootEntity childRoot, IUnitOfWork parentUoW, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
			where TChildRootEntity : class, IDomainObject, new();

		/// <summary>
		/// Создаем дочерний Unit of Work с новым экземляром сущности, не использующий коммит при сохранении Root
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TChildRootEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		IUnitOfWorkGeneric<TChildRootEntity> CreateWithNewChildRoot<TChildRootEntity>(IUnitOfWork parentUoW, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
			where TChildRootEntity : class, IDomainObject, new();
	}
}