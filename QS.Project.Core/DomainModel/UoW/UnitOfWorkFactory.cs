using System.Runtime.CompilerServices;
using QS.DomainModel.Entity;
using QS.Project.DB;

namespace QS.DomainModel.UoW
{
	//FIXME Статический класс пока оставлен для совместимости со старым кодом и пока не совсем понятно как реализовать остальные объекты без статики
	// в идеале в конечном итоге придти к тому чтобы фабрика и остальные классы были не статичны, что позмолит в одном приложении одновременно
	// работать с несколькими паралельными соединениями с разными базами. То есть вся статика в работе с базой должна быть убрана и переведена на внедрение зависимостей.
	// это будет полезно как минимум для запуска паралельных тестов.
	public static class UnitOfWorkFactory
	{
		private static readonly IUnitOfWorkFactory unitOfWorkFactory = new DefaultUnitOfWorkFactory(new DefaultSessionProvider());

		public static IUnitOfWorkFactory GetDefaultFactory => unitOfWorkFactory;

		/// <summary>
		/// Создаем Unit of Work без корренной сущьности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public static IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
		{
			return unitOfWorkFactory.CreateWithoutRoot(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
			return unitOfWorkFactory.CreateForRoot<TEntity>(id, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
			return unitOfWorkFactory.CreateWithNewRoot<TEntity>(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
			return unitOfWorkFactory.CreateWithNewRoot<TEntity>(entity, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем дочерний Unit of Work не использующий коммит при сохранении Root 
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TChildRootEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TChildRootEntity> CreateForChildRoot<TChildRootEntity>(TChildRootEntity childRoot, IUnitOfWork parentUoW, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
			where TChildRootEntity : class, IDomainObject, new()
		{
			return unitOfWorkFactory.CreateForChildRoot<TChildRootEntity>(childRoot, parentUoW, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		/// <summary>
		/// Создаем дочерний Unit of Work с новым экземляром сущности, не использующий коммит при сохранении Root
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TChildRootEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public static IUnitOfWorkGeneric<TChildRootEntity> CreateWithNewChildRoot<TChildRootEntity>(IUnitOfWork parentUoW, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
			where TChildRootEntity : class, IDomainObject, new()
		{
			return unitOfWorkFactory.CreateWithNewChildRoot<TChildRootEntity>(parentUoW, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}
	}
}