using System;
using System.Collections.Generic;
using System.Reflection;
using QSOrmProject;
using System.Collections;
using System.Linq;

namespace QSHistoryLog
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

			fields.RemoveAll (f => (f.GetCustomAttributes (typeof(IgnoreHistoryCloneAttribute), true)).Length > 0);

			foreach (FieldInfo f in fields)
			{
				if( (f.GetCustomAttributes (typeof(HistoryTraceGoDeepAttribute), true)).Length > 0)
				{
					var value = f.GetValue (sourceObject);

					if(value is IList)
					{
						if( (f.GetCustomAttributes (typeof(HistoryDeepCloneItemsAttribute), true)).Length > 0)
							f.SetValue (destinationObject, CloneListDeep (value as IList));
						else
							f.SetValue (destinationObject, CloneList(value as IList));
					}
					else 
						f.SetValue (destinationObject, Clone (value));
				}
				else
					f.SetValue (destinationObject, f.GetValue (sourceObject));
			}
		}

		public static IList CloneList(IList list)
		{
			var itemType = list.GetType ().GetGenericArguments () [0];
			var listType = typeof(List<>);
			var constructedListType = listType.MakeGenericType(itemType);

			var newlist = (IList)Activator.CreateInstance (constructedListType);
			foreach(var item in list)
			{
				newlist.Add (item);
			}
			return newlist;
		}

		public static IList CloneListDeep(IList list)
		{
			var itemType = list.GetType ().GetGenericArguments () [0];
			var listType = typeof(List<>);
			var constructedListType = listType.MakeGenericType(itemType);

			var newlist = (IList)Activator.CreateInstance (constructedListType);
			foreach(var item in list)
			{
				newlist.Add (CloneSingle (item));
			}
			return newlist;
		}


		private static T CloneSingle<T>(T cloneObject)
		{
			if (cloneObject is ICloneable)
				return (T)(cloneObject as ICloneable).Clone ();
			if (typeof(T).IsClass)
				return (T)Clone ((object)cloneObject);
			throw new NotSupportedException ();
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

