using QS.DomainModel.UoW;
using QS.Project.DB;

namespace QS.DB
{
	/// <summary>
	/// Базовый класс для тестирования работы с базой.
	/// *Внимание* на при создании нового UoW, создается новая чистая база данных в памяти
	/// по конфигруации.
	/// Этот класс в отличии от InMemoryDBTestFixtureBase рассчитывает на то что все необходимое для работы Nh уже сконфигурировано глобально.
	/// </summary>
	public abstract class InMemoryDBGlobalConfigTestFixtureBase
	{
		protected IUnitOfWorkFactory UnitOfWorkFactory;

		/// <summary>
		/// Инициализация только фабрики uow без инициализации Nh
		/// </summary>
		public void InitialiseUowFactory()
		{
			UnitOfWorkFactory = new DefaultUnitOfWorkFactory(new InMemoryDBTestSessionProvider(OrmConfig.NhConfig));
		}
	}
}
