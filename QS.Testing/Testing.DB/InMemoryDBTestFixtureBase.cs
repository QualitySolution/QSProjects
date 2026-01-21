using Autofac;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate.Event;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System.Reflection;

namespace QS.Testing.DB {
	/// <summary>
	/// Базовый класс для тестирования работы с базой.
	/// *Внимание* на при создании нового UoW, создается новая чистая база данных в памяти
	/// по конфигурации.
	/// Вызовите NewSessionWithSameDB для создания нового Uow к уже имеющейся базе, для создания новых соединение первое, созданное к это базе, не должно быть закрыто.
	/// Вызовите NewSessionWithNewDB для переключения в режим по умолчанию, новый UoW с новой чистой базой.
	/// </summary>
	public abstract class InMemoryDBTestFixtureBase
	{
		protected IUnitOfWorkFactory UnitOfWorkFactory;
		protected InMemoryDBTestSessionProvider inMemoryDBTestSessionProvider;
		protected NHibernate.Cfg.Configuration NhConfiguration;

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
			if(uowEventsTracker != null) {
				var listeners = new[] { uowEventsTracker };
				fluentConfig.ExposeConfiguration(cfg => {
					cfg.AppendListeners(ListenerType.PostLoad, listeners);
					cfg.AppendListeners(ListenerType.PreLoad, listeners);
					cfg.AppendListeners(ListenerType.PostDelete, listeners);
					cfg.AppendListeners(ListenerType.PostUpdate, listeners);
					cfg.AppendListeners(ListenerType.PostInsert, listeners);
				});
			}

			NhConfiguration = fluentConfig.BuildConfiguration();
			var sessionFactory = NhConfiguration.BuildSessionFactory();
			inMemoryDBTestSessionProvider = new InMemoryDBTestSessionProvider(NhConfiguration, sessionFactory);
			var builder = new ContainerBuilder();
			builder.RegisterInstance(inMemoryDBTestSessionProvider).As<ISessionProvider>();
			builder.RegisterType<SingleUowEventsTracker>().AsSelf();
			var container = builder.Build();
			UnitOfWorkFactory = new TrackedUnitOfWorkFactory(container);
		}

		#region NotifyChange
		GlobalUowEventsTracker uowEventsTracker;
		protected AppLevelChangeListener ChangeWatcher;

		protected void NotifyChangeEnable()
		{
			uowEventsTracker = new GlobalUowEventsTracker();
			var listener = new AppLevelChangeListener(new DefaultTrackerActionInvoker());
			GlobalUowEventsTracker.RegisterGlobalListener(listener);
			SingleUowEventsTracker.RegisterSingleUowListnerFactory(listener);
			ChangeWatcher = listener;
		}
		#endregion

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
