using System;

namespace QSOrmProject
{
	[AttributeUsage (AttributeTargets.Class)]
	public class OrmSubjectAttribute : Attribute
	{
		public string JournalName;
		public string ObjectName;
		public ReferenceButtonMode DefaultJournalMode = ReferenceButtonMode.CanAll;

		public OrmSubjectAttribute ()
		{
		}

		[Obsolete("Используйте заполнение атрибута через параметры.")]
		public OrmSubjectAttribute (string journalName)
		{
			JournalName = journalName;
		}
	}

	[AttributeUsage (AttributeTargets.Class)]
	public class OrmDefaultIsFilteredAttribute : Attribute
	{
		public bool DefaultIsFiltered;

		public OrmDefaultIsFilteredAttribute (bool defaultIsFiltered)
		{
			DefaultIsFiltered = defaultIsFiltered;
		}
	}

	[AttributeUsage (AttributeTargets.Property)]
	public class PropertyChangedAlsoAttribute : Attribute
	{
		public string[] PropertiesNames;

		public PropertyChangedAlsoAttribute (params string[] propertiesNames)
		{
			PropertiesNames = propertiesNames;
		}
	}

}
