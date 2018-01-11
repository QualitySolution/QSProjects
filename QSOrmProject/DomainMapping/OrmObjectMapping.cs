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

		private bool? defaultUseSlider;

		public bool? DefaultUseSlider
		{
			get
			{
				return defaultUseSlider;
			}
		}

		public string EditPermisionName { get; set;}

		private TableView<TEntity> tableView;

		public ITableView TableView {
			get {
				return tableView;
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

		public Func<TEntity[], Gtk.Menu> GetPopupMenuFunc { get; set;}

		public bool PopupMenuExist { get{ return GetPopupMenuFunc != null; }}

		private OrmObjectMapping()
		{
			
		}

		public Gtk.Menu GetPopupMenu(object[] selected)
		{
			if (GetPopupMenuFunc == null)
				return null;

			return GetPopupMenuFunc(selected.Cast<TEntity>().ToArray());
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

		/// <summary>
		/// Указываем диалог по умолчанию, для открытия сущьности
		/// </summary>
		public OrmObjectMapping<TEntity> Dialog<TDialog>()
		{
			this.dialogClass = typeof(TDialog);
			return this;
		}

		/// <summary>
		/// Указываем диалог по умолчанию, для открытия сущьности
		/// </summary>
		public OrmObjectMapping<TEntity> Dialog(Type dialogClass)
		{
			this.dialogClass = dialogClass;
			return this;
		}

		/// <summary>
		/// Указываем указываем журнал по умолчанию, для выбора сущьностей.
		/// </summary>
		public OrmObjectMapping<TEntity> JournalFilter<TFilter>()
		{
			this.refFilterClass = typeof(TFilter);
			return this;
		}

		/// <summary>
		/// Указываем указываем журнал по умолчанию, для выбора сущьностей.
		/// </summary>
		public OrmObjectMapping<TEntity> JournalFilter(Type filterClass)
		{
			this.refFilterClass = filterClass;
			return this;
		}

		/// <summary>
		/// Устанавливаем необходимые права для редактирования сущьности.
		/// </summary>
		public OrmObjectMapping<TEntity> EditPermision(string permisionName)
		{
			this.EditPermisionName = permisionName;
			return this;
		}

		/// <summary>
		/// Добавляем в контекстное меню, которое будет отображаться в журнале сущьностей.
		/// </summary>
		public OrmObjectMapping<TEntity> PopupMenu(Func<TEntity[], Gtk.Menu> getMenuFunc)
		{
			this.GetPopupMenuFunc = getMenuFunc;
			return this;
		}

		/// <summary>
		/// Устанавливаем будет ли использоваться слайдер для журналов сущьности.
		/// </summary>
		public OrmObjectMapping<TEntity> UseSlider(bool? useSlider)
		{
			this.defaultUseSlider = useSlider;
			return this;
		}

		public TableView<TEntity> DefaultTableView()
		{
			tableView = new TableView<TEntity> (this);
			return tableView;
		}

		#endregion
	}
}

