﻿using QS.DomainModel.UoW;
using QS.Project.DB;

namespace QS.Testing.DB
{
	/// <summary>
	/// Базовый класс для тестирования работы с базой.
	/// *Внимание* на при создании нового UoW, создается новая чистая база данных в памяти
	/// по конфигурации.
	/// Вызовите NewSessionWithSameDB для создания нескольких Uow к одной базе, первое созданное uow к базе соединение не должно быть закрыто.
	/// Вызовите NewSessionWithNewDB для переключения в режим по умолчанию, новый UoW с новой чистой базой.
	/// Этот класс в отличие от InMemoryDBTestFixtureBase рассчитывает на то что все необходимое для работы Nh уже сконфигурировано глобально.
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
			UnitOfWorkFactory = new DefaultUnitOfWorkFactory(inMemoryDBTestSessionProvider);
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
