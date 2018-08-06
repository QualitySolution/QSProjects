using System;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using KellermanSoftware.CompareNetObjects;
using QSOrmProject;
using System.Reflection;
using System.Collections;

namespace QSHistoryLog
{
	public class DomainObjectComparer : ClassComparer
	{
		public DomainObjectComparer (RootComparer rootComparer) : base(rootComparer)
		{
		}

		public override bool IsTypeMatch (Type type1, Type type2)
		{
			var classMatch = base.IsTypeMatch (type1, type2);
			if (type1 != null && type2 != null && !classMatch)
				return false;

			if (type1 == null && type2 == null)
				return false;

			if (type1 == typeof(IDomainObject) || type2 == typeof(IDomainObject))
				return true;

			PropertyInfo prop1 = null, prop2 = null;
			if(type1 != null)
				prop1 = type1.GetProperty ("Id");

			if(type2 != null)
				prop2 = type2.GetProperty ("Id");

			return  prop1 != null || prop2 != null;
		}

		public override void CompareType(CompareParms parms)
		{
			if(parms.ParentObject1 == null || parms.ParentObject2 == null)
			{
				base.CompareType (parms);
				return;
			}

			PropertyInfo prop1 = null, prop2 = null;
			if(parms.Object1 != null)
				prop1 = parms.Object1.GetType ().GetProperty ("Id");
			if(parms.Object2 != null)
				prop2 = parms.Object2.GetType ().GetProperty ("Id");

			if (prop1 != null && prop2 != null &&
			    (int)prop1.GetValue (parms.Object1, null) == (int)prop2.GetValue (parms.Object2, null)) {
				if (parms.ParentObject1 is IList)
					base.CompareType (parms);
				return;
			}

			Difference difference = new Difference
			{
				ParentObject1 =  new WeakReference(parms.ParentObject1),
				ParentObject2 =  new WeakReference(parms.ParentObject2),
				PropertyName = parms.BreadCrumb,
				Object1Value = parms.Object1 == null ? String.Empty : HistoryMain.GetObjectTilte (parms.Object1),
				Object2Value = parms.Object2 == null ? String.Empty : HistoryMain.GetObjectTilte (parms.Object2),
				ChildPropertyName = "Id",
				Object1 = new WeakReference(parms.Object1),
				Object2 = new WeakReference(parms.Object2)
			};

			AddDifference(parms.Result, difference);
		}
	}
}

