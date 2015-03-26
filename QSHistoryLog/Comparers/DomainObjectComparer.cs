using System;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using KellermanSoftware.CompareNetObjects;
using QSOrmProject;

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
			if (!classMatch)
				return false;
			if (type1 == typeof(IDomainObject) && type2 == typeof(IDomainObject))
				return true;

			var prop1 = type1.GetProperty ("Id");
			var prop2 = type2.GetProperty ("Id");
			return prop1 != null && prop2 != null;
		}

		public override void CompareType(CompareParms parms)
		{
			if(parms.ParentObject1 == null || parms.ParentObject2 == null)
			{
				base.CompareType (parms);
				return;
			}

			var prop1 = parms.Object1.GetType ().GetProperty ("Id");
			var prop2 = parms.Object2.GetType ().GetProperty ("Id");

			if ((int)prop1.GetValue (parms.Object1, null) == (int)prop2.GetValue (parms.Object2, null))
				return;

			Difference difference = new Difference
			{
				ParentObject1 =  new WeakReference(parms.ParentObject1),
				ParentObject2 =  new WeakReference(parms.ParentObject2),
				PropertyName = parms.BreadCrumb,
				Object1Value = HistoryMain.GetObjectTilte (parms.Object1),
				Object2Value = HistoryMain.GetObjectTilte (parms.Object2),
				ChildPropertyName = "Id",
				Object1 = new WeakReference(parms.Object1),
				Object2 = new WeakReference(parms.Object2)
			};

			AddDifference(parms.Result, difference);
		}
	}
}

