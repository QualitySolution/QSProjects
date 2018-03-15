using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Cfg;
using Gtk;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Proxy;
using NLog;
using QS.DomainModel;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using QSTDI;

namespace QSOrmProject
{
	public static class OrmMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static Configuration ormConfig;
		internal static FluentConfiguration fluenConfig;
		internal static ISessionFactory Sessions;
		public static List<IOrmObjectMapping> ClassMappingList = new List<IOrmObjectMapping>();
		private static List<DelayedNotifyLink> delayedNotifies = new List<DelayedNotifyLink>();
		private static DateTime lastCleaning;
		public static int Count = 0;

		public static ISession OpenSession(IInterceptor interceptor = null)
		{
			ISession session = interceptor == null ? Sessions.OpenSession() : Sessions.OpenSession(interceptor);
			session.FlushMode = FlushMode.Commit;
			return session;
		}

		#region Конфигруация ORM

		public static Configuration OrmConfig {
			get {
				if (ormConfig == null && fluenConfig != null)
					ormConfig = fluenConfig.BuildConfiguration();
				return ormConfig;
			}
			set {
				ormConfig = value;
			}
		}

		/// <summary>
		/// Настройка Nhibernate только с загрузкой мапинга из XML.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="assemblies">Assemblies.</param>
		[Obsolete("Не поддеживает настройку перехвата событий необходимых для работы HistoryLog и IBusinessObject")]
		public static void ConfigureOrm(string connectionString, string[] assemblies)
		{
			ormConfig = new Configuration();

			ormConfig.Configure();
			ormConfig.SetProperty("connection.connection_string", connectionString);

			foreach (string ass in assemblies) {
				ormConfig.AddAssembly(ass);
			}

			Sessions = ormConfig.BuildSessionFactory();
		}

		/// <summary>
		/// Настройка Nhibernate c одновременной загрузкой мапинга из XML и Fluent конфигураций.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="assemblies">Assemblies.</param>
		[Obsolete("Не поддеживает настройку перехвата событий необходимых для работы HistoryLog и IBusinessObject")]
		public static void ConfigureOrm(string connectionString, System.Reflection.Assembly[] assemblies)
		{
			ormConfig = new Configuration();

			ormConfig.Configure();
			ormConfig.SetProperty("connection.connection_string", connectionString);
			//ormConfig.AppendListeners(NHibernate.Event.ListenerType.Load, new object[] {});
			//ormConfig.EventListeners.LoadEventListeners = new ILoadEventListener[] { new DebugLoadListener(), new DefaultLoadEventListener() };

			fluenConfig = Fluently.Configure(ormConfig);

			fluenConfig.Mappings(m => {
				foreach (var ass in assemblies) {
					m.HbmMappings.AddFromAssembly(ass);
					m.FluentMappings.AddFromAssembly(ass);
				}
			});

			Sessions = fluenConfig.BuildSessionFactory();
		}

		/// <summary>
		/// Настройка Nhibernate только с Fluent конфигураций.
		/// </summary>
		/// <param name="assemblies">Assemblies.</param>
		public static void ConfigureOrm(FluentNHibernate.Cfg.Db.IPersistenceConfigurer database, System.Reflection.Assembly[] assemblies, Action<Configuration> exposeConfiguration = null)
		{
			fluenConfig = Fluently.Configure().Database(database);

			fluenConfig.Mappings(m => {
				foreach (var ass in assemblies) {
					m.FluentMappings.AddFromAssembly(ass);
				}
			});

			var trackerListener = new NhEventListener();
			fluenConfig.ExposeConfiguration(cfg => {
				cfg.AppendListeners(NHibernate.Event.ListenerType.PostLoad, new[] { trackerListener });
				cfg.AppendListeners(NHibernate.Event.ListenerType.PreLoad, new[] { trackerListener });
				cfg.AppendListeners(NHibernate.Event.ListenerType.PostDelete, new[] { trackerListener });
			});

			if (exposeConfiguration != null)
				fluenConfig.ExposeConfiguration(exposeConfiguration);

			Sessions = fluenConfig.BuildSessionFactory();
		}

		#endregion
		#region Классы сущностей

		public static OrmObjectMapping<TEntity> AddObjectDescription<TEntity>()
		{
			var map = OrmObjectMapping<TEntity>.Create();
			ClassMappingList.Add(map);
			return map;
		}

		public static IOrmObjectMapping AddObjectDescription(IOrmObjectMapping map)
		{
			ClassMappingList.Add(map);
			return map;
		}

		public static Type GetDialogType(System.Type objectClass)
		{
			if (ClassMappingList == null)
				throw new NullReferenceException("ORM Модуль не настроен. Нужно создать ClassMapingList.");

			if (objectClass.GetInterface(typeof(INHibernateProxy).FullName) != null)
				objectClass = objectClass.BaseType;

			IOrmObjectMapping map = ClassMappingList.Find(c => c.ObjectClass == objectClass);
			if (map == null) {
				logger.Warn("Диалог для типа {0} не найден.", objectClass);
				return null;
			} else
				return map.DialogClass;
		}

		public static IOrmObjectMapping GetObjectDescription(System.Type type)
		{
			if (type.GetInterface(typeof(INHibernateProxy).FullName) != null)
				type = type.BaseType;

			return OrmMain.ClassMappingList.Find(m => m.ObjectClass == type);
		}

		public static OrmObjectMapping<TEntity> GetObjectDescription<TEntity>()
		{
			if (ClassMappingList == null)
				return null;
			return OrmMain.ClassMappingList.Find(m => m.ObjectClass == typeof(TEntity)) as OrmObjectMapping<TEntity>;
		}

		#endregion
		#region Уведомления об изменениях

		/// <summary>
		/// Уведомляем всех подписчиков об изменении указанных объектов.
		/// Объекты в списке могут быть разных типов, метод разделит списки уведомлений, по типам объектов.
		/// </summary>
		/// <param name="subject">Subject.</param>
		public static void NotifyObjectUpdated(params object[] updatedSubjects)
		{
			if (ClassMappingList == null)
				return;
			// Чистим список от удаленных объектов.
			if (DateTime.Now.Subtract(lastCleaning).TotalSeconds > 1) {
				delayedNotifies.RemoveAll(d => d.ParentObject == null || d.ChangedObject == null);
				lastCleaning = DateTime.Now;
			}

			foreach (Type subjectType in updatedSubjects.Select(s => NHibernateProxyHelper.GuessClass(s)).Distinct()) {
				IOrmObjectMapping map = ClassMappingList.Find(m => m.ObjectClass == subjectType);
				if (map != null)
					map.RaiseObjectUpdated(updatedSubjects.Where(s => NHibernateProxyHelper.GuessClass(s) == subjectType).ToArray());
				else
					logger.Warn("В ClassMapingList класс {0} объекта не найден. Поэтому событие обновления не вызвано.", subjectType);

				// Отсылаем уведомления дочерним объектам если они есть.
				foreach (DelayedNotifyLink link in delayedNotifies.FindAll(l => OrmMain.Equals(l.ParentObject, updatedSubjects[0]))) {
					NotifyObjectUpdated(link.ChangedObject);
					delayedNotifies.Remove(link);
				}

			}
		}

		/// <summary>
		/// Просим отложенно уведомить подписчиков об изменении дочернего объекта,
		/// при наступлении события обновления родителя.
		/// </summary>
		/// <param name="withObject">Уведомление сработает в момент обновления этого объекта.</param>
		/// <param name="subject">Subject.</param>
		public static void DelayedNotifyObjectUpdated(object withObject, object subject)
		{
			if (!delayedNotifies.Exists(d => d.ChangedObject == subject && d.ParentObject == withObject)) {
				delayedNotifies.Add(new DelayedNotifyLink(withObject, subject));
			}
		}

		#endregion
		#region Диалоги сущностей

		public static IOrmDialog FindMyDialog(Widget child)
		{
			if (child.Parent == null)
				return null;
			else if (child.Parent is IOrmDialog)
				return child.Parent as IOrmDialog;
			else if (child.Parent.IsTopLevel)
				return null;
			else
				return FindMyDialog(child.Parent);
		}

		public static String GetDBTableName(System.Type objectClass)
		{
			return ormConfig.GetClassMapping(objectClass).RootTable.Name;
		}

		public static string GenerateDialogHashName<TEntity>(int id) where TEntity : IDomainObject
		{
			return GenerateDialogHashName(typeof(TEntity), id);
		}

		public static string GenerateDialogHashName(Type clazz, int id)
		{
			if (!typeof(IDomainObject).IsAssignableFrom(clazz))
				throw new ArgumentException("Тип должен реализовывать интерфейс IDomainObject", "clazz");

			return String.Format("{0}_{1}", clazz.Name, id);
		}

		/// <summary>
		/// Создает произвольный диалог для класса из доменной модели приложения.
		/// </summary>
		/// <returns>Виджет с интерфейсом ITdiDialog</returns>
		/// <param name="objectClass">Класс объекта для которого нужно создать диалог.</param>
		/// <param name="parameters">Параметры конструктора диалога.</param>
		public static ITdiDialog CreateObjectDialog(System.Type objectClass, params object[] parameters)
		{
			System.Type dlgType = GetDialogType(objectClass);
			if (dlgType == null) {
				InvalidOperationException ex = new InvalidOperationException(
												   String.Format("Для класса {0} нет привязанного диалога.", objectClass));
				logger.Error(ex);
				throw ex;
			}
			Type[] paramTypes = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				paramTypes[i] = parameters[i].GetType();
			}

			System.Reflection.ConstructorInfo ci = dlgType.GetConstructor(paramTypes);
			if (ci == null) {
				InvalidOperationException ex = new InvalidOperationException(
												   String.Format("Конструктор для диалога {0} с параметрами({1}) не найден.", dlgType.ToString(), NHibernate.Util.CollectionPrinter.ToString(paramTypes)));
				logger.Error(ex);
				throw ex;
			}
			logger.Debug("Вызываем конструктор диалога {0} c параметрами {1}.", dlgType.ToString(), NHibernate.Util.CollectionPrinter.ToString(paramTypes));
			return (ITdiDialog)ci.Invoke(parameters);
		}

		/// <summary>
		/// Создаёт диалог для конкретного объекта доменной модели приложения.
		/// </summary>
		/// <returns>Виджет с интерфейсом ITdiDialog</returns>
		/// <param name="entity">Объект для которого нужно создать диалог.</param>
		public static ITdiDialog CreateObjectDialog(object entity)
		{
			return CreateObjectDialog(NHibernateProxyHelper.GuessClass(entity), entity);
		}

		#endregion
		#region Удаление сущностей

		public static bool DeleteObject(string table, int id)
		{
			var delete = new Deletion.DeleteCore();
			try {
				return delete.RunDeletion(table, id);
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
		}

		public static bool DeleteObject(Type objectClass, int id, IUnitOfWork uow = null)
		{
			var delete = uow == null ? new Deletion.DeleteCore() : new Deletion.DeleteCore(uow);
			try {
				return delete.RunDeletion(objectClass, id);
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
		}

		public static bool DeleteObject<TEntity>(int id, IUnitOfWork uow = null)
		{
			return DeleteObject(typeof(TEntity), id, uow);
		}

		public static bool DeleteObject(object subject, IUnitOfWork uow = null)
		{
			if (!(subject is IDomainObject))
				throw new ArgumentException("Класс должен реализовывать интерфейс IDomainObject", "subject");
			var objectClass = NHibernateProxyHelper.GuessClass(subject);
			int id = (subject as IDomainObject).Id;
			var delete = uow == null ? new Deletion.DeleteCore() : new Deletion.DeleteCore(uow);
			try {
				return delete.RunDeletion(objectClass, id);
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
		}

		#endregion
	}

	internal class DelayedNotifyLink
	{
		private WeakReference parentObject;

		public object ParentObject {
			get { return parentObject.Target; }
		}

		private WeakReference changedObject;

		public object ChangedObject {
			get { return changedObject.Target; }
		}

		public DelayedNotifyLink(object parentObject, object changedObject)
		{
			this.parentObject = new WeakReference(parentObject);
			this.changedObject = new WeakReference(changedObject);
		}
	}
}

