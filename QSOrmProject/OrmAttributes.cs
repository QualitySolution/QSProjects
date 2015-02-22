using System;

namespace QSOrmProject
{
	[AttributeUsage(AttributeTargets.Class)]
	public class OrmSubjectAttribute : Attribute 
	{
		public string JournalName;
		public OrmSubjectAttribute(string journalName) { JournalName = journalName; }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class OrmDefaultIsFiltered : Attribute 
	{
		public bool DefaultIsFiltered;
		public OrmDefaultIsFiltered(bool defaultIsFiltered) { DefaultIsFiltered = defaultIsFiltered; }
	}

}
