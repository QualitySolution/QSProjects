using System;

namespace QSOrmProject
{
	[AttributeUsage (AttributeTargets.Class)]
	public class OrmDefaultIsFilteredAttribute : Attribute
	{
		public bool DefaultIsFiltered;

		public OrmDefaultIsFilteredAttribute (bool defaultIsFiltered)
		{
			DefaultIsFiltered = defaultIsFiltered;
		}
	}
}
