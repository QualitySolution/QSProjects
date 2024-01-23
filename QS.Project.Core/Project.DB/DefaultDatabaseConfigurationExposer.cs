using NHibernate.Cfg;
using System;

namespace QS.Project.DB {
	public class DefaultDatabaseConfigurationExposer : IDatabaseConfigurationExposer {
		private readonly Action<Configuration> _exposeConfigurationAction;

		public DefaultDatabaseConfigurationExposer(Action<Configuration> exposeConfigurationAction) {
			_exposeConfigurationAction = exposeConfigurationAction ?? throw new ArgumentNullException(nameof(exposeConfigurationAction));
		}

		public void ExposeConfiguration(Configuration config) {
			_exposeConfigurationAction.Invoke(config);
		}
	}
}
