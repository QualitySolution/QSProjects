using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.Project.DB;
using System;
using System.Runtime.CompilerServices;

namespace QS.DomainModel.UoW {
	public class TrackedUnitOfWorkFactory : IUnitOfWorkFactory {
		private readonly ILifetimeScope scope;

		public TrackedUnitOfWorkFactory(ILifetimeScope scope) {
			this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		/// <summary>
		/// Создаем Unit of Work без конкретной сущности.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var sessionProvider = scope.Resolve<ISessionProvider>();
			var tracker = scope.Resolve<SingleUowEventsTracker>();
			return new UnitOfWorkWithoutRoot(sessionProvider, title, tracker);
		}

		/// <summary>
		/// Создаем Unit of Work загружая сущность по id.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateForRoot<TEntity>(int id, string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) where TEntity : class, IDomainObject, new() {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var sessionProvider = scope.Resolve<ISessionProvider>();
			var tracker = scope.Resolve<SingleUowEventsTracker>();
			return new UnitOfWork<TEntity>(sessionProvider, id, title, tracker);
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) where TEntity : class, IDomainObject, new() {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var sessionProvider = scope.Resolve<ISessionProvider>();
			var tracker = scope.Resolve<SingleUowEventsTracker>();
			return new UnitOfWork<TEntity>(sessionProvider, title, tracker);
		}

		/// <summary>
		/// Создаем Unit of Work с новым экземпляром сущности переданным в виде аргумента
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		/// <typeparam name="TEntity">Тип объекта доменной модели, должен реализовывать интерфейс IDomainObject.</typeparam>
		public IUnitOfWorkGeneric<TEntity> CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) where TEntity : class, IDomainObject, new() {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var sessionProvider = scope.Resolve<ISessionProvider>();
			var tracker = scope.Resolve<SingleUowEventsTracker>();
			return new UnitOfWork<TEntity>(sessionProvider, entity, title, tracker);
		}
	}
}
