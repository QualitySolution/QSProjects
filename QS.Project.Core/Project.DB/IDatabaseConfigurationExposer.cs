using NHibernate.Cfg;

namespace QS.Project.DB {
	public interface IDatabaseConfigurationExposer {
		void ExposeConfiguration(Configuration config);
	}
}
