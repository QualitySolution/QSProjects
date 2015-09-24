using System;
using System.Linq;
using QSOrmProject.UpdateNotification;

namespace QSOrmProject.DomainMapping
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
				if (simpleDislpay == null)
					return refSearchFields;
				else
					return simpleDislpay.SearchFields.ToArray ();
			}
		}

		private string refColumnMappings;

		public string RefColumnMappings {
			get {
				if(simpleDislpay == null)
					return refColumnMappings;
				else
				{
					string mapping = "{" + typeof(TEntity).FullName + "}";
					foreach(var pair in simpleDislpay.ColumnsFields)
					{
						mapping += String.Format (" {0}[{1}];", pair.Value, pair.Key);
					}
					return mapping;
				}
			}
		}

		private SimpleDisplay<TEntity> simpleDislpay;

		public event EventHandler<OrmObjectUpdatedEventArgs> ObjectUpdated;
		public event EventHandler<OrmObjectUpdatedGenericEventArgs<TEntity>> ObjectUpdatedGeneric;

		public bool SimpleDialog
		{
			get
			{
				return (DialogClass == null);
			}
		}

		private OrmObjectMapping()
		{
			
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

		#region FluentConfig

		public static OrmObjectMapping<TEntity> Create()
		{
			return new OrmObjectMapping<TEntity> ();
		}

		public OrmObjectMapping<TEntity> Dialog(Type dialogClass)
		{
			this.dialogClass = dialogClass;
			return this;
		}

		public OrmObjectMapping<TEntity> JournalFilter(Type filterClass)
		{
			this.refFilterClass = filterClass;
			return this;
		}

		public SimpleDisplay<TEntity> SimpleDisplay()
		{
			simpleDislpay = new SimpleDisplay<TEntity> (this);
			return simpleDislpay;
		}

		#endregion
	}
}

