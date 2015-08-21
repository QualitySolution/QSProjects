using System;
using NHibernate;
using QSProjectsLib;

namespace QSOrmProject
{
	public static class DomainHelper
	{
		public static string GetObjectTilte(object value)
		{
			var prop = value.GetType ().GetProperty ("Title");
			if (prop != null) {
				return prop.GetValue (value, null).ToString();
			}

			prop = value.GetType ().GetProperty ("Name");
			if (prop != null) {
				return prop.GetValue (value, null).ToString();
			}

			return value.ToString ();
		}

		public static int GetId(object value)
		{
			if (value == null)
				throw new ArgumentNullException ();
			if (value is IDomainObject)
				return (value as IDomainObject).Id;

			var prop = value.GetType ().GetProperty ("Id");
			if (prop == null)
				throw new ArgumentException ("Для работы метода тип {0}, должен иметь свойство Id.");

			return (int)prop.GetValue (value, null);
		}

		public static bool EqualDomainObjects (object obj1, object obj2)
		{
			if (obj1 == null || obj2 == null)
				return false;

			if (NHibernateUtil.GetClass (obj1) != NHibernateUtil.GetClass (obj2))
				return false;

			if (obj1 is IDomainObject)
				return (obj1 as IDomainObject).Id == (obj2 as IDomainObject).Id;

			return obj1.Equals (obj2);
		}

		public static SubjectName GetSubjectNames(object subject)
		{
			return GetSubjectNames (subject.GetType ());
		}

		public static SubjectName GetSubjectNames(Type subjectType)
		{
			object[] att = subjectType.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0) {
				return (att [0] as OrmSubjectAttribute).AllNames;
			} else
				return null;
		}
	}
}

