using System;

namespace QSOrmProject
{
	[AttributeUsage(AttributeTargets.Class)]
	public class OrmSubjectAttibutes : Attribute 
	{
		public string JournalName;
		public OrmSubjectAttibutes(string journalName) { JournalName = journalName; }
	}

}
