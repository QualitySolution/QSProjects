using System.Reflection;
using NHibernate.Cfg;
using QS.DomainModel.UoW;
using QS.Project.DB;

namespace QS.DB
{
	/// <summary>
	/// Базовый класс для тестирования работы с базой.
	/// *Внимание* на при создании нового UoW, создается новая чистая база данных в памяти
	/// по конфигруации.
	/// </summary>
	public abstract class InMemoryDBTestFixtureBase
	{
		protected static Configuration configuration;
		protected static IUnitOfWorkFactory UnitOfWorkFactory;

		public static void InitialiseNHibernate(params Assembly[] assemblies)
		{
			if (configuration != null)
				return;

			var db_config = FluentNHibernate.Cfg.Db.MonoSqliteConfiguration.Standard.InMemory();

			OrmConfig.ConfigureOrm(db_config, assemblies);
			configuration = OrmConfig.NhConfig;
			UnitOfWorkFactory = new DefaultUnitOfWorkFactory(new InMemoryDBTestSessionProvider(configuration));
		}
	}
}
