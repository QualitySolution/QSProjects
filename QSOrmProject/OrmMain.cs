using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Cfg;
using Gtk;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Proxy;
using NLog;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using QSTDI;

namespace QSOrmProject
{
	public static class OrmMain
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		internal static Configuration ormConfig;
		internal static FluentConfiguration fluenConfig;
		internal static ISessionFactory Sessions;
		public static List<IOrmObjectMapping> ClassMappingList;
		private static List<DelayedNotifyLink> delayedNotifies = new List<DelayedNotifyLink> ();
		private static DateTime lastCleaning;

		public static ISession OpenSession ()
		{
			ISession session = Sessions.OpenSession ();
			//session.FlushMode = FlushMode.Never;
			return session;
		}

		/// <summary>
		/// Настройка Nhibernate только с загрузкой мапинга из XML.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="assemblies">Assemblies.</param>
		public static void ConfigureOrm (string connectionString, string[] assemblies)
		{
			ormConfig = new Configuration (); 

			ormConfig.Configure ();
			ormConfig.SetProperty ("connection.connection_string", connectionString);

			foreach (string ass in assemblies) {
				ormConfig.AddAssembly (ass);
			}

			Sessions = ormConfig.BuildSessionFactory ();
		}

		/// <summary>
		/// Настройка Nhibernate c одновременной загрузкой мапинга из XML и Fluent конфигураций.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="assemblies">Assemblies.</param>
		public static void ConfigureOrm (string connectionString, System.Reflection.Assembly[] assemblies)
		{
			ormConfig = new Configuration (); 

			ormConfig.Configure ();
			ormConfig.SetProperty ("connection.connection_string", connectionString);

			fluenConfig = Fluently.Configure (ormConfig);

			fluenConfig.Mappings (m => {
				foreach (var ass in assemblies)
				{
					m.HbmMappings.AddFromAssembly (ass);
					m.FluentMappings.AddFromAssembly (ass);
				}
			});

			Sessions = fluenConfig.BuildSessionFactory ();
		}

		public static Type GetDialogType (System.Type objectClass)
		{
			if (ClassMappingList == null)
				throw new NullReferenceException ("ORM Модуль не настроен. Нужно создать ClassMapingList.");

			if (objectClass.GetInterface (typeof(INHibernateProxy).FullName) != null)
				objectClass = objectClass.BaseType;

			IOrmObjectMapping map = ClassMappingList.Find (c => c.ObjectClass == objectClass);
			if (map == null) {
				logger.Warn ("Диалог для типа {0} не найден.", objectClass);
				return null;
			} else
				return map.DialogClass;
		}

		public static IOrmObjectMapping GetObjectDescription (System.Type type)
		{
			if (type.GetInterface (typeof(INHibernateProxy).FullName) != null)
				type = type.BaseType;

			return OrmMain.ClassMappingList.Find (m => m.ObjectClass == type);
		}

		public static OrmObjectMapping<TEntity> GetObjectDescription<TEntity> ()
		{
			if (ClassMappingList == null)
				return null;
			return OrmMain.ClassMappingList.Find (m => m.ObjectClass == typeof(TEntity)) as OrmObjectMapping<TEntity>;
		}

		/// <summary>
		/// Уведомляем всех подписчиков об изменении указанных объектов.
		/// Объекты в списке могут быть разных типов, метод разделит списки уведомлений, по типам объектов.
		/// </summary>
		/// <param name="subject">Subject.</param>
		public static void NotifyObjectUpdated (params object[] updatedSubjects)
		{
			if (ClassMappingList == null)
				return;
			// Чистим список от удаленных объектов.
			if (DateTime.Now.Subtract (lastCleaning).TotalSeconds > 1) {
				delayedNotifies.RemoveAll (d => d.ParentObject == null || d.ChangedObject == null);
				lastCleaning = DateTime.Now;
			}

			foreach (Type subjectType in updatedSubjects.Select(s => NHibernateUtil.GetClass (s)).Distinct ()) {
				IOrmObjectMapping map = ClassMappingList.Find (m => m.ObjectClass == subjectType);
				if (map != null)
					map.RaiseObjectUpdated (updatedSubjects.Where (s => NHibernateUtil.GetClass (s) == subjectType).ToArray ());
				else
					logger.Warn ("В ClassMapingList тип объекта не найден. Поэтому событие обновления не вызвано.");

				// Отсылаем уведомления дочерним объектам если они есть.
				foreach (DelayedNotifyLink link in delayedNotifies.FindAll (l => OrmMain.Equals (l.ParentObject, updatedSubjects[0]))) {
					NotifyObjectUpdated (link.ChangedObject);
					delayedNotifies.Remove (link);
				}

			}				
		}

		/// <summary>
		/// Просим отложенно уведомить подписчиков об изменении дочернего объекта,
		/// при наступлении события обновления родителя.
		/// </summary>
		/// <param name="withObject">Уведомление сработает в момент обновления этого объекта.</param>
		/// <param name="subject">Subject.</param>
		public static void DelayedNotifyObjectUpdated (object withObject, object subject)
		{
			if (!delayedNotifies.Exists (d => d.ChangedObject == subject && d.ParentObject == withObject)) {
				delayedNotifies.Add (new DelayedNotifyLink (withObject, subject));
			}
		}

		public static IOrmDialog FindMyDialog (Widget child)
		{
			if (child.Parent == null)
				return null;
			else if (child.Parent is IOrmDialog)
				return child.Parent as IOrmDialog;
			else if (child.Parent.IsTopLevel)
				return null;
			else
				return FindMyDialog (child.Parent);
		}

		public static String GetDBTableName (System.Type objectClass)
		{
			return ormConfig.GetClassMapping (objectClass).RootTable.Name;
		}

		/// <summary>
		/// Создает произвольный диалог для класса из доменной модели приложения.
		/// </summary>
		/// <returns>Виджет с интерфейсом ITdiDialog</returns>
		/// <param name="objectClass">Класс объекта для которого нужно создать диалог.</param>
		/// <param name="parameters">Параметры конструктора диалога.</param>
		public static ITdiDialog CreateObjectDialog (System.Type objectClass, params object[] parameters)
		{
			System.Type dlgType = GetDialogType (objectClass);
			if (dlgType == null) {
				InvalidOperationException ex = new InvalidOperationException (
					                               String.Format ("Для класса {0} нет привязанного диалога.", objectClass));
				logger.Error (ex);
				throw ex;
			}
			Type[] paramTypes = new Type[parameters.Length];	
			for (int i = 0; i < parameters.Length; i++) {
				paramTypes [i] = parameters [i].GetType ();
			}

			System.Reflection.ConstructorInfo ci = dlgType.GetConstructor (paramTypes);
			if (ci == null) {
				InvalidOperationException ex = new InvalidOperationException (
					                               String.Format ("Конструктор для диалога {0} с параметрами({1}) не найден.", dlgType.ToString (), NHibernate.Util.CollectionPrinter.ToString (paramTypes)));
				logger.Error (ex);
				throw ex;
			}
			logger.Debug ("Вызываем конструктор диалога {0} c параметрами {1}.", dlgType.ToString (), NHibernate.Util.CollectionPrinter.ToString (paramTypes));
			return (ITdiDialog)ci.Invoke (parameters);
		}

		/// <summary>
		/// Создаёт диалог для конкретного объекта доменной модели приложения.
		/// </summary>
		/// <returns>Виджет с интерфейсом ITdiDialog</returns>
		/// <param name="entity">Объект для которого нужно создать диалог.</param>
		public static ITdiDialog CreateObjectDialog (object entity)
		{
			return CreateObjectDialog (NHibernateUtil.GetClass (entity), entity);
		}

		public static bool DeleteObject(string table, int id)
		{
			var delete = new Deletion.DeleteCore();
			try{
				return delete.RunDeletion(table, id);	
			}catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
		}

		public static bool DeleteObject(Type objectClass, int id)
		{
			var delete = new Deletion.DeleteCore();
			try{
				return delete.RunDeletion(objectClass, id);
			}catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
		}

		public static bool DeleteObject(object subject)
		{
			if (!(subject is IDomainObject))
				throw new ArgumentException("Класс должен реализовывать интерфейс IDomainObject", "subject");
			var objectClass = NHibernateUtil.GetClass(subject);
			int id = (subject as IDomainObject).Id;
			var delete = new Deletion.DeleteCore();
			try{
				return delete.RunDeletion(objectClass, id);
			}catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog("Ошибка удаления.", logger, ex);
				return false;
			}
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

		public DelayedNotifyLink (object parentObject, object changedObject)
		{
			this.parentObject = new WeakReference (parentObject);
			this.changedObject = new WeakReference (changedObject);
		}
	}
}

