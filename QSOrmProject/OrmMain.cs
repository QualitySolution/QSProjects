using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Proxy;
using NLog;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tdi;
using QSOrmProject.DomainMapping;
using QSProjectsLib;

namespace QSOrmProject
{
	public static class OrmMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public static List<IOrmObjectMapping> ClassMappingList = new List<IOrmObjectMapping>();
		private static List<DelayedNotifyLink> delayedNotifies = new List<DelayedNotifyLink>();
		private static DateTime lastCleaning;
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
			var delete = uow == null ? new QS.Deletion.DeleteCore() : new QS.Deletion.DeleteCore(uow);
			var delList = delete.GetDeletionList(objectClass, id);

			foreach(var item in delList) {
				result.Add(new DeletionObject() { Id = item.ItemId, Type = item.ItemClass });
			}
			return result;
		}

		public static bool DeleteObject(string table, int id)
		{
			var delete = new QS.Deletion.DeleteCore();
			try {
				return delete.RunDeletion(table, id);
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
		}

		public static bool DeleteObject(Type objectClass, int id, IUnitOfWork uow = null, System.Action beforeDeletion = null)
		{
			var delete = uow == null ? new QS.Deletion.DeleteCore() : new QS.Deletion.DeleteCore(uow);
			delete.BeforeDeletion = beforeDeletion;
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

		/// <summary>
		/// Удаляем объект вместе с зависимостями с отображением пользователю диалога показывающего что еще будет удалено.
		/// </summary>
		/// <returns><c>true</c>, if object was deleted, <c>false</c> otherwise.</returns>
		/// <param name="subject">Удяляемый объект</param>
		/// <param name="uow">UnitOfWork в котором нужно выполнить удаление. Если не передать будет созданн новый UnitOfWork.</param>
		/// <param name="beforeDeletion">Метод который нужно выполнить перед удалением, если пользователь подтвердит удаление.</param>
		public static bool DeleteObject(object subject, IUnitOfWork uow = null, System.Action beforeDeletion = null)
		{
			if (!(subject is IDomainObject))
				throw new ArgumentException("Класс должен реализовывать интерфейс IDomainObject", "subject");
			var objectClass = NHibernateProxyHelper.GuessClass(subject);
			int id = (subject as IDomainObject).Id;
			var delete = uow == null ? new QS.Deletion.DeleteCore() : new QS.Deletion.DeleteCore(uow);
			delete.BeforeDeletion = beforeDeletion;
			try {
				return delete.RunDeletion(objectClass, id);
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
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
			//FIXME Временные пробросы на этап перехода на QS.Poject
			QS.Project.Repositories.UserRepository.GetCurrentUserId = () => QSMain.User.Id;
			QS.Project.DB.Connection.GetConnectionString = () => QSMain.ConnectionString;
			QS.Project.DB.Connection.GetConnectionDB = () => QSMain.ConnectionDB;
			QS.DomainModel.Config.DomainConfiguration.GetEntityConfig = (clazz) => GetObjectDescription(clazz) as IEntityConfig;
			QS.Deletion.DeleteHelper.DeleteEntity = (clazz, id) => DeleteObject(clazz, id);

			QS.DomainModel.UoW.UnitOfWorkBase.NotifyObjectUpdated = NotifyObjectUpdated;
			QSProjectsLib.QSMain.RunOrmDeletion += RunDeletionFromProjectLib;
		}
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

