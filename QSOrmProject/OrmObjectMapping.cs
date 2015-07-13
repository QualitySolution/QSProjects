using System;
using System.Linq;
using QSOrmProject.UpdateNotification;

namespace QSOrmProject
{
	public class OrmObjectMapping<TEntity> : IOrmObjectMapping
	{
		public Type ObjectClass {
			get {
				return typeof(TEntity);
			}
		}

		Type dialogClass;

		public Type DialogClass {
			get {
				return dialogClass;
			}
		}

		private Type refFilterClass;

		public Type RefFilterClass {
			get {
				return refFilterClass;
			}
		}

		private string[] refSearchFields;

		public string[] RefSearchFields {
			get {
				return refSearchFields;
			}
		}

		private string refColumnMappings;

		public string RefColumnMappings {
			get {
				return refColumnMappings;
			}
		}

		public event EventHandler<OrmObjectUpdatedEventArgs> ObjectUpdated;
		public event EventHandler<OrmObjectUpdatedGenericEventArgs<TEntity>> ObjectUpdatedGeneric;

		public bool SimpleDialog
		{
			get
			{
				return (DialogClass == null);
			}
		}

		public OrmObjectMapping(System.Type dialogClass)
		{
			this.dialogClass = dialogClass;
			refColumnMappings = String.Empty;
		}

		public OrmObjectMapping(System.Type dialogClass, string columnMaping) : this(dialogClass)
		{
			refColumnMappings = columnMaping;
		}

		public OrmObjectMapping(System.Type dialogClass, string columnMaping, string[] searchFields) : this(dialogClass, columnMaping)
		{
			this.refSearchFields = searchFields;
		}

		public OrmObjectMapping(System.Type dialogClass, System.Type filterClass, string columnMaping, string[] searchFields) : this(dialogClass, columnMaping, searchFields)
		{
			this.refFilterClass = filterClass;
		}
			
		public void RaiseObjectUpdated(params object[] updatedSubjects)
		{
			if (ObjectUpdatedGeneric != null)
				ObjectUpdatedGeneric(this, 
					new OrmObjectUpdatedGenericEventArgs<TEntity>(updatedSubjects.Cast<TEntity> ().ToArray ()));

			if (ObjectUpdated != null)
				ObjectUpdated(this, new OrmObjectUpdatedEventArgs(updatedSubjects));
		}
	}
}

