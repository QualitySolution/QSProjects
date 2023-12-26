using System;
using System.Runtime.CompilerServices;

namespace QS.DomainModel.UoW {
	[Obsolete("Лучше явно использовать TrackedUnitOfWorkFactory")]
	public class DefaultUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly TrackedUnitOfWorkFactory factory;

		public DefaultUnitOfWorkFactory(TrackedUnitOfWorkFactory factory)
		{
			this.factory = factory;
		}

		public IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0) {
			return factory.CreateWithoutRoot(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		IUnitOfWorkGeneric<TEntity> IUnitOfWorkFactory.CreateForRoot<TEntity>(int id, string userActionTitle, string callerMemberName, string callerFilePath, int callerLineNumber) {
			return factory.CreateForRoot<TEntity>(id, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		IUnitOfWorkGeneric<TEntity> IUnitOfWorkFactory.CreateWithNewRoot<TEntity>(string userActionTitle, string callerMemberName, string callerFilePath, int callerLineNumber) {
			return factory.CreateWithNewRoot<TEntity>(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}

		IUnitOfWorkGeneric<TEntity> IUnitOfWorkFactory.CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle, string callerMemberName, string callerFilePath, int callerLineNumber) {
			return factory.CreateWithNewRoot<TEntity>(entity, userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
		}
	}
}
