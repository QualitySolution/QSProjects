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
		/// Creates new object of type T and clones cloneObject into it.
		/// </summary>
		/// <returns>The clone of the object.</returns>
		/// <param name="cloneObject">The object for cloning.</param>
		/// <typeparam name="T">Type of the object.</typeparam>
		public static T Clone<T> (T cloneObject) where T : class
		{
			T newObject = Activator.CreateInstance<T> ();
			FieldsCopy<T> (cloneObject, ref newObject);
			return newObject;
		}
	}
}

