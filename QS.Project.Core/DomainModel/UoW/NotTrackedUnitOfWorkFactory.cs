using QS.DomainModel.Entity;
using QS.Project.DB;
using System.Runtime.CompilerServices;

namespace QS.DomainModel.UoW {
	public class NotTrackedUnitOfWorkFactory : IUnitOfWorkFactory {
		private readonly ISessionProvider sessionProvider;

		public NotTrackedUnitOfWorkFactory(ISessionProvider sessionProvider) {
			this.sessionProvider = sessionProvider;
		}

		/// <summary>
		/// Создаем Unit of Work без конкретной сущности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			return new UnitOfWorkWithoutRoot(sessionProvider, title);
		}

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id, string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) where TEntity : class, IDomainObject, new() {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var uow = new UnitOfWork<TEntity>(sessionProvider, id, title);
			return uow;
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) where TEntity : class, IDomainObject, new() {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var uow = new UnitOfWork<TEntity>(sessionProvider, title);
			return uow;
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) where TEntity : class, IDomainObject, new() {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var uow = new UnitOfWork<TEntity>(sessionProvider, entity, title);
			return uow;
		}
	}
}
