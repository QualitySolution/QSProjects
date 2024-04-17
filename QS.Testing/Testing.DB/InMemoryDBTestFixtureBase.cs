using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using QS.DomainModel.UoW;
using System.Reflection;

namespace QS.Testing.DB {
	/// <summary>
	/// Базовый класс для тестирования работы с базой.
	/// *Внимание* на при создании нового UoW, создается новая чистая база данных в памяти
	/// по конфигурации.
	/// Вызовите NewSessionWithSameDB для создание нового Uow к уже имеющейся базе, для создание новых первое созданное к это базе соединение не должно быть закрыто.
	/// Вызовите NewSessionWithNewDB для переключения в режим по умолчанию, новый UoW с новой чистой базой.
	/// </summary>
	public abstract class InMemoryDBTestFixtureBase
	{
		protected IUnitOfWorkFactory UnitOfWorkFactory;
		protected InMemoryDBTestSessionProvider inMemoryDBTestSessionProvider;

		/// <summary>
		/// Полная инициализация всего необходимого для тестирования в Nh
		/// </summary>
		public void InitialiseNHibernate(params Assembly[] assemblies)
		{
			var sqlConfiguration = MonoSqliteConfiguration.Standard.InMemory();
			var fluentConfig = Fluently.Configure().Database(sqlConfiguration);
			fluentConfig.Mappings(m => {
				foreach(var assembly in assemblies) {
					m.FluentMappings.AddFromAssembly(assembly);
				}
			});

			var configuration = fluentConfig.BuildConfiguration();
			var sessionFactory = configuration.BuildSessionFactory();
			inMemoryDBTestSessionProvider = new InMemoryDBTestSessionProvider(configuration, sessionFactory);
			UnitOfWorkFactory = new NotTrackedUnitOfWorkFactory(inMemoryDBTestSessionProvider);
		}

		/// <summary>
		/// Внимание! Чтобы следующие UoW могли быть созданы, первый созданный Uow не должен закрывать соединение с базой.
		/// </summary>
		public void NewSessionWithSameDB()
		{
			inMemoryDBTestSessionProvider.UseSameDB = true;
		}

		public void NewSessionWithNewDB()
		{
			inMemoryDBTestSessionProvider.UseSameDB = false;
		}
	}
}
