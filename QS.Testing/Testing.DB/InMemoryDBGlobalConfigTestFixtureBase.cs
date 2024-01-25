using QS.DomainModel.UoW;
using QS.Project.DB;

namespace QS.Testing.DB {
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
		private readonly InMemoryDBTestSessionProvider provider;

		public InMemoryDBGlobalConfigTestFixtureBase(InMemoryDBTestSessionProvider provider) {
			this.provider = provider ?? throw new System.ArgumentNullException(nameof(provider));
		}

		/// <summary>
		/// Инициализация только фабрики uow без инициализации Nh
		/// </summary>
		public void InitialiseUowFactory()
		{
			UnitOfWorkFactory = new NotTrackedUnitOfWorkFactory(provider);
		}

		public void NewSessionWithSameDB()
		{
			provider.UseSameDB = true;
		}

		public void NewSessionWithNewDB()
		{
			provider.UseSameDB = false;
		}
	}
}
