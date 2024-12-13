using Autofac;
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
		/// Создаем Unit of Work с отслеживанием.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public IUnitOfWork Create(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			var sessionProvider = scope.Resolve<ISessionProvider>();
			var tracker = scope.Resolve<SingleUowEventsTracker>();
			return new UnitOfWork(sessionProvider, title, tracker);
		}
	}
}
