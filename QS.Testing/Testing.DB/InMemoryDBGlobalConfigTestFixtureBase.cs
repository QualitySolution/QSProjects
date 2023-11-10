using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Testing.Gui;

namespace QS.Testing.DB
{
	/// <summary>
	/// Базовый класс для тестирования работы с базой.
	/// *Внимание* на при создании нового UoW, создается новая чистая база данных в памяти
	/// по конфигурации.
	/// Вызовите NewSessionWithSameDB для создание нового Uow к уже имеющейся базе, для создание новых первое созданное к это базе соединение не должно быть закрыто.
	/// Вызовите NewSessionWithNewDB для переключения в режим по умолчанию, новый UoW с новой чистой базой.
	/// Этот класс в отличии от InMemoryDBTestFixtureBase рассчитывает на то что все необходимое для работы Nh уже сконфигурировано глобально.
	/// </summary>
	public abstract class InMemoryDBGlobalConfigTestFixtureBase
	{
		protected IUnitOfWorkFactory UnitOfWorkFactory;
		protected InMemoryDBTestSessionProvider inMemoryDBTestSessionProvider;

		/// <summary>
		/// Инициализация только фабрики uow без инициализации Nh
		/// </summary>
		public void InitialiseUowFactory()
		{
			inMemoryDBTestSessionProvider = new InMemoryDBTestSessionProvider(OrmConfig.NhConfig);
			UnitOfWorkFactory = new DefaultUnitOfWorkFactory(inMemoryDBTestSessionProvider, new ServerThreadDispatcher());
		}

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
