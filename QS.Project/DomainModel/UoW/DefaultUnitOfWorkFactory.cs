using System.Runtime.CompilerServices;
using QS.DomainModel.Entity;
using QS.Project.DB;

namespace QS.DomainModel.UoW
{
	public class DefaultUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionProvider SessionProvider;

		public DefaultUnitOfWorkFactory(ISessionProvider sessionProvider)
		{
			SessionProvider = sessionProvider;
		}

		/// <summary>
		/// Создаем Unit of Work без корренной сущьности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
		{
            var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
            return new UnitOfWorkWithoutRoot(SessionProvider, title);
		}

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
            var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
            var uow = new UnitOfWork<TEntity>(SessionProvider, id, title);
			return uow;
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
            var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
            var uow = new UnitOfWork<TEntity>(SessionProvider, title);
			return uow;
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) where TEntity : class, IDomainObject, new()
		{
            var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
            var uow = new UnitOfWork<TEntity>(SessionProvider, entity, title);
			return uow;
		}

		/// <summary>
		/// Создаем дочерний Unit of Work не использующий коммит при сохранении Root 
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TChildRootEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TChildRootEntity> CreateForChildRoot<TChildRootEntity>(TChildRootEntity childRoot, IUnitOfWork parentUoW, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
			where TChildRootEntity : class, IDomainObject, new()
		{
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var uow = new UnitOfWorkChild<TChildRootEntity>(null, childRoot, parentUoW, title);
			uow.SessionScopeEntitySaved += (sender, e) => { parentUoW.RaiseSessionScopeEntitySaved(e.UpdatedSubjects); };
			return uow;
		}

		/// <summary>
		/// Создаем дочерний Unit of Work с новым экземляром сущности, не использующий коммит при сохранении Root
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TChildRootEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TChildRootEntity> CreateWithNewChildRoot<TChildRootEntity>(IUnitOfWork parentUoW, string userActionTitle = null, [CallerMemberName]string callerMemberName = null, [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0)
			where TChildRootEntity : class, IDomainObject, new()
		{
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var uow = new UnitOfWorkChild<TChildRootEntity>(null, new TChildRootEntity(), parentUoW, title);
			uow.SessionScopeEntitySaved += (sender, e) => { parentUoW.RaiseSessionScopeEntitySaved(e.UpdatedSubjects); };
			return uow;
		}
	}
}