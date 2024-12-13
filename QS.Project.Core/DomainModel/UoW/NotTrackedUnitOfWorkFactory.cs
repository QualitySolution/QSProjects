using QS.Project.DB;
using System.Runtime.CompilerServices;

namespace QS.DomainModel.UoW {
	public class NotTrackedUnitOfWorkFactory : IUnitOfWorkFactory {
		private readonly ISessionProvider sessionProvider;

		public NotTrackedUnitOfWorkFactory(ISessionProvider sessionProvider) {
			this.sessionProvider = sessionProvider;
		}

		/// <summary>
		/// Создаем Unit of Work без отслеживания.
		/// </summary>
		/// <returns>UnitOfWork.</returns>
		public IUnitOfWork Create(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) {
			var title = new UnitOfWorkTitle(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			return new UnitOfWork(sessionProvider, title);
		}
	}
}
