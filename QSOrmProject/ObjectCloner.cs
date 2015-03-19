using System;
using System.Collections.Generic;
using System.Reflection;

namespace QSOrmProject
{
	
	public static class ObjectCloner
	{
		public static void ReflectionClone<T> (T cloneObject, ref T newObject) where T : class
		{
			Type type = cloneObject.GetType ();
			List<FieldInfo> fields = new List<FieldInfo> ();

			while (type != null) {
				fields.AddRange (type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
				type = type.BaseType;
			}
			foreach (FieldInfo f in fields)
				f.SetValue (newObject, f.GetValue (cloneObject));
		}
	}
}

