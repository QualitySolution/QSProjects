using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using QSTDI;

namespace QSOrmProject
{
	public static class OrmMain
	{
		public static ISessionFactory Sessions;
		public static List<OrmObjectMaping> ClassMapingList;

		public static void ConfigureOrm(string connectionString ,string[] assemblies)
		{
			var c = new Configuration(); 

			c.Configure();
			c.SetProperty("connection.connection_string", connectionString);

			foreach(string ass in assemblies)
			{
				c.AddAssembly(ass);
			}

			Sessions = c.BuildSessionFactory ();
		}

		public static Type GetDialogType(System.Type objectClass)
		{
			if (ClassMapingList == null)
				throw new NullReferenceException("ORM Модуль не настроен. Нужно создать ClassMapingList.");
			return ClassMapingList.Find(c => c.ObjectClass == objectClass).DialogClass;
		}

		public static void NotifyObjectUpdated(object subject)
		{
			OrmObjectMaping map = ClassMapingList.Find(m => m.ObjectClass == subject.GetType());
			if (map != null)
				map.RaiseObjectUpdated(subject);
		}
	}

	public class OrmObjectMaping
	{
		public System.Type ObjectClass;
		public System.Type DialogClass;
		public event EventHandler<OrmObjectUpdatedEventArgs> ObjectUpdated;

		public OrmObjectMaping(System.Type objectClass, System.Type dialogClass)
		{
			ObjectClass = objectClass;
			DialogClass = dialogClass;
		}

		public void RaiseObjectUpdated(int id)
		{
			if (ObjectUpdated != null)
				ObjectUpdated(this, new OrmObjectUpdatedEventArgs(id));
		}

		public void RaiseObjectUpdated(object subject)
		{
			if (ObjectUpdated != null)
				ObjectUpdated(this, new OrmObjectUpdatedEventArgs(subject));
		}
	}

	public class OrmObjectUpdatedEventArgs : EventArgs
	{
		public int Id { get; private set; }
		public object Subject { get; private set; }

		public OrmObjectUpdatedEventArgs(int id)
		{
			Id = id;
		}

		public OrmObjectUpdatedEventArgs(object subject)
		{
			Subject= subject;
		}
	}

}

