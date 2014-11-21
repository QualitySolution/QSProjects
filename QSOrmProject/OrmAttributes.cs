using System;

namespace QSOrmProject
{
	[AttributeUsage(AttributeTargets.Class)]
	public class OrmSubjectAttributes : Attribute 
	{
		public string JournalName;
		public OrmSubjectAttributes(string journalName) { JournalName = journalName; }
	}

}
