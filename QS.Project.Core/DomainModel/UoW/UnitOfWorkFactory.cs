using Autofac;
using QS.DomainModel.Entity;
using System.Runtime.CompilerServices;

namespace QS.DomainModel.UoW {
	//FIXME Статический класс пока оставлен для совместимости со старым кодом и пока не совсем понятно как реализовать остальные объекты без статики
	// в идеале в конечном итоге прийти к тому чтобы фабрика и остальные классы были не статичны, что позволит в одном приложении одновременно
	// работать с несколькими параллельными соединениями с разными базами. То есть вся статика в работе с базой должна быть убрана и переведена на внедрение зависимостей.
	// это будет полезно как минимум для запуска параллельных тестов.
	public static class UnitOfWorkFactory
	{
		public static ILifetimeScope Scope { get; set; }

		public static IUnitOfWorkFactory GetDefaultFactory => Scope.Resolve<IUnitOfWorkFactory>();

		/// <summary>
		/// Создаем Unit of Work без конкретной сущности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public static IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
		{
			return GetDefaultFactory.CreateWithoutRoot(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
			return GetDefaultFactory.CreateForRoot<TEntity>(id, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
			return GetDefaultFactory.CreateWithNewRoot<TEntity>(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
			return GetDefaultFactory.CreateWithNewRoot<TEntity>(entity, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}
	}
}
