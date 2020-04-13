using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using NHibernate.Proxy;
using NLog;
using QS.Deletion;
using QS.Deletion.Views;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Tdi;
using QS.Utilities;
using QS.Views.Resolve;
using QSOrmProject.DomainMapping;
using QSProjectsLib;

namespace QSOrmProject
{
	public static class OrmMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public static List<IOrmObjectMapping> ClassMappingList = new List<IOrmObjectMapping>();
		public static int Count = 0;

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
		public static void NotifyObjectUpdated(EntityChangeEvent[] updatedSubjects)
		{
			if (ClassMappingList == null)
				return;

			var eventsGroups = updatedSubjects
				.Where(ev => ev.EventType == TypeOfChangeEvent.Insert || ev.EventType == TypeOfChangeEvent.Update)
				.GroupBy(s => s.EntityClass);

			foreach (var entityClassGroup in eventsGroups) {
				IOrmObjectMapping map = ClassMappingList.Find(m => m.ObjectClass == entityClassGroup.Key);
				if (map != null)
					map.RaiseObjectUpdated(entityClassGroup.Select(x => x.Entity).ToArray());
				else
					logger.Warn("В ClassMapingList класс {0} объекта не найден. Поэтому событие обновления не вызвано.", entityClassGroup.Key);
			}
		}

		#endregion
		#region Диалоги сущностей

		[Obsolete("Используйте аналогичную функцию из DialogHelper. Из OrmMain этот функционал будет удален.")]
		public static string GenerateDialogHashName<TEntity>(int id) where TEntity : IDomainObject
		{
			return GenerateDialogHashName(typeof(TEntity), id);
		}

		[Obsolete("Используйте аналогичную функцию из DialogHelper. Из OrmMain этот функционал будет удален.")]
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
				if(parameters[i] is INHibernateProxy)
					paramTypes[i] = NHibernateProxyHelper.GuessClass(parameters[i]);
				else
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

		public class DeletionObject
		{
			public Type Type { get; set; }
			public uint Id { get; set; }
		}

		public static List<DeletionObject> GetDeletionObjects(Type objectClass, int id, IUnitOfWork uow = null)
		{
			var result = new List<DeletionObject>();
			var delete = new DeleteCore(DeleteConfig.Main, uow);
			delete.PrepareDeletion(objectClass, id, new System.Threading.CancellationTokenSource().Token);

			foreach(var item in delete.DeletedItems) {
				result.Add(new DeletionObject() { Id = item.Id, Type = item.ClassType });
			}
			return result;
		}

		[Obsolete("Используйте сервис удаления напряму. Например DeleteEntityGUIService.")]
		public static bool DeleteObject(string table, int id)
		{
			var info = DeleteConfig.Main.GetDeleteInfo(table);
			if (info == null)
				throw new InvalidOperationException($"Правила для удаления объектов таблицы {table} не найдено.");

			return DeleteObject(info.ObjectClass, id);
		}

		[Obsolete("Используйте сервис удаления напряму. Например DeleteEntityGUIService.")]
		public static bool DeleteObject(Type objectClass, int id, IUnitOfWork uow = null, System.Action beforeDeletion = null)
		{
			try {
				//Здесь так все криво, просто чтобы сделать независимую реализация чисто для совместимости со старым кодом.
				var builder = new ContainerBuilder();
				IContainer container = null;
				builder.Register((ctx) => new AutofacViewModelsGtkPageFactory(container)).As<IViewModelsPageFactory>();
				builder.RegisterType<GtkWindowsNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
				builder.RegisterType<DeleteEntityGUIService>().AsSelf();
				builder.Register(x => DeleteConfig.Main).AsSelf();
				builder.RegisterType<GtkMessageDialogsInteractive>().As<IInteractiveMessage>();
				builder.RegisterType<GtkQuestionDialogsInteractive>().As<IInteractiveQuestion>();
				builder.RegisterModule(new DeletionAutofacModule());
				builder.Register(ctx => new ClassNamesBaseGtkViewResolver(Assembly.GetAssembly(typeof(DeletionView)))).As<IGtkViewResolver>();
				container = builder.Build();

				var deleteSerive = container.Resolve<DeleteEntityGUIService>();
				var deletion = deleteSerive.DeleteEntity(objectClass, id, uow, beforeDeletion);

				while (deletion.DeletionExecuted == null)
					GtkHelper.WaitRedraw();

				return deletion.DeletionExecuted.Value;

			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
		}

		[Obsolete("Используйте сервис удаления напряму. Например DeleteEntityGUIService.")]
		public static bool DeleteObject<TEntity>(int id, IUnitOfWork uow = null)
		{
			return DeleteObject(typeof(TEntity), id, uow);
		}

		/// <summary>
		/// Удаляем объект вместе с зависимостями с отображением пользователю диалога показывающего что еще будет удалено.
		/// </summary>
		/// <returns><c>true</c>, if object was deleted, <c>false</c> otherwise.</returns>
		/// <param name="subject">Удяляемый объект</param>
		/// <param name="uow">UnitOfWork в котором нужно выполнить удаление. Если не передать будет созданн новый UnitOfWork.</param>
		/// <param name="beforeDeletion">Метод который нужно выполнить перед удалением, если пользователь подтвердит удаление.</param>
		[Obsolete("Используйте сервис удаления напряму. Например DeleteEntityGUIService.")]
		public static bool DeleteObject(object subject, IUnitOfWork uow = null, System.Action beforeDeletion = null)
		{
			if (!(subject is IDomainObject))
				throw new ArgumentException("Класс должен реализовывать интерфейс IDomainObject", "subject");
			var objectClass = NHibernateProxyHelper.GuessClass(subject);
			int id = (subject as IDomainObject).Id;
			return DeleteObject(objectClass, id, beforeDeletion: beforeDeletion);
		}

		#endregion

		#region Переходный период на QS.Project

		/// <summary>
		/// Необходимо для интеграции с библиотекой QSProjectsLib
		/// </summary>
		static void RunDeletionFromProjectLib(object sender, QSProjectsLib.QSMain.RunOrmDeletionEventArgs e)
		{
			e.Result = DeleteObject(e.TableName, e.ObjectId);
		}

		#endregion

		static OrmMain()
		{
			//FIXME Временные пробросы на этап перехода на QS.Project
			QS.Project.Repositories.UserRepository.GetCurrentUserId = () => QSMain.User.Id;
			QS.Project.DB.Connection.GetConnectionString = () => QSMain.ConnectionString;
			QS.Project.DB.Connection.GetConnectionDB = () => QSMain.ConnectionDB;
			QS.DomainModel.Config.DomainConfiguration.GetEntityConfig = (clazz) => GetObjectDescription(clazz) as IEntityConfig;
			QS.Deletion.DeleteHelper.DeleteEntity = (clazz, id) => DeleteObject(clazz, id);

			QS.DomainModel.NotifyChange.NotifyConfiguration.Enable(); //Включаем чтобы не падали старые проекта. По хорошему каждый проект должне отдельно включать.
			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribeOnAll(NotifyObjectUpdated);

			QSProjectsLib.QSMain.RunOrmDeletion += RunDeletionFromProjectLib;
		}
	}
}

