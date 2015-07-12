using System;
using System.Collections.Generic;
using System.Reflection;

namespace QSOrmProject
{
	public static class ObjectCloner
	{
		/// <summary>
		/// Copies every field in source object into destination object.
		/// </summary>
		/// <param name="sourceObject">Source object to copy fields from.</param>
		/// <param name="destinationObject">Destination object to copy fields to.</param>
		/// <typeparam name="T">Type of the object.</typeparam>
		public static void FieldsCopy<T> (T sourceObject, ref T destinationObject) where T : class
		{
			Type type = sourceObject.GetType ();
			List<FieldInfo> fields = new List<FieldInfo> ();

			while (type != null) {
				fields.AddRange (type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
				type = type.BaseType;
			}
			foreach (FieldInfo f in fields)
				f.SetValue (destinationObject, f.GetValue (sourceObject));
		}

		/// <summary>
		/// Сравниваем все поля на равенство, ссылки на доменные объекты сравниваем по ID.
		/// </summary>
		/// <returns><c>true</c> если поля объектов одинаковы.</returns>
		/// <param name="object1">Object1.</param>
		/// <param name="object2">Object2.</param>
		public static bool CompareFields<T> (T object1, T object2) where T : class
		{
			Type type = object1.GetType ();
			List<FieldInfo> fields = new List<FieldInfo> ();

			while (type != null) {
				fields.AddRange (type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
				type = type.BaseType;
			}
			foreach (FieldInfo f in fields)
			{
				object value = f.GetValue (object1);
				if(value is IDomainObject)
				{
					if (!DomainHelper.EqualDomainObjects (value, f.GetValue (object2)))
						return false;
				}
				else
				{
					if (!value.Equals(f.GetValue (object2)))
						return false;
				}

			}
			return true;
		}

		/// <summary>
		/// Creates new object of type T and clones cloneObject into it.
		/// </summary>
		/// <returns>The clone of the object.</returns>
		/// <param name="cloneObject">The object for cloning.</param>
		/// <typeparam name="T">Type of the object.</typeparam>
		public static T Clone<T> (T cloneObject) where T : class , new()
		{
			T newObject = Activator.CreateInstance<T> ();
			FieldsCopy<T> (cloneObject, ref newObject);
			return newObject;
		}
	}
}

