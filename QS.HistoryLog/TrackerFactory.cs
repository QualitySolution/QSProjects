using System;
using MySql.Data.MySqlClient;
using QS.DomainModel.Tracking;

namespace QS.HistoryLog
{
	public class TrackerFactory : ISingleUowEventsListnerFactory
	{
		private readonly MySqlConnectionStringBuilder connectionStringBuilder;

		public TrackerFactory(MySqlConnectionStringBuilder connectionStringBuilder) {
			this.connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
		}

		public ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow)
		{
			return new HibernateTracker(connectionStringBuilder.GetConnectionString(true));
		}
	}
}
