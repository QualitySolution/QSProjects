using System;

namespace QSOrmProject
{
	[AttributeUsage(AttributeTargets.Class)]
	public class OrmSubjectAttribute : Attribute 
	{
		public string JournalName;
		public string Name;
		public OrmSubjectAttribute(string journalName) { JournalName = journalName; }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class OrmDefaultIsFilteredAttribute : Attribute 
	{
		public bool DefaultIsFiltered;
		public OrmDefaultIsFilteredAttribute(bool defaultIsFiltered) { DefaultIsFiltered = defaultIsFiltered; }
	}

}
