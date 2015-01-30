using System;

namespace QSOrmProject
{
	public class OrmObjectMapping
	{
		public System.Type ObjectClass;
		public System.Type DialogClass;
		public string[] RefSearchFields;
		public string RefColumnMappings;
		public event EventHandler<OrmObjectUpdatedEventArgs> ObjectUpdated;

		public bool SimpleDialog
		{
			get
			{
				return (DialogClass == null);
			}
		}

		public OrmObjectMapping(System.Type objectClass, System.Type dialogClass)
		{
			ObjectClass = objectClass;
			DialogClass = dialogClass;
			RefColumnMappings = String.Empty;
		}

		public OrmObjectMapping(System.Type objectClass, System.Type dialogClass, string columnMaping) : this(objectClass, dialogClass)
		{
			RefColumnMappings = columnMaping;
		}

		public OrmObjectMapping(System.Type objectClass, System.Type dialogClass, string columnMaping, string[] searchFields) : this(objectClass, dialogClass, columnMaping)
		{
			RefSearchFields = searchFields;
		}

		public void RaiseObjectUpdated(int id)
		{
			if (ObjectUpdated != null)
				ObjectUpdated(this, new OrmObjectUpdatedEventArgs(id));
		}

		public void RaiseObjectUpdated(object subject)
		{
			if (ObjectUpdated != null)
				ObjectUpdated(this, new OrmObjectUpdatedEventArgs(subject));
		}

	}
}

