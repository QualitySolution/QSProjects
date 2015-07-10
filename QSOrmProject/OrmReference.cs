using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections;
using System.Reflection;
using Gtk;
using NHibernate;
using NLog;
using QSOrmProject.UpdateNotification;
using QSTDI;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrmReference : Gtk.Bin, ITdiJournal
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private IUnitOfWork uow;
		private ICriteria objectsCriteria;
		private System.Type objectType;
		private ObservableFilterListView filterView;
		private IReferenceFilter filterWidget;
		private uint totalSearchFinished;
		private DateTime searchStarted;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<OrmReferenceObjectSectedEventArgs> ObjectSelected;

		public IUnitOfWork Uow {
			get
			{
				if (uow == null)
					uow = UnitOfWorkFactory.CreateWithoutRoot ();
				return uow;
			}
			set { uow = value; }
		}

		System.Type filterClass;

		public System.Type FilterClass {
			get { return filterClass; }
			set {
				if (filterClass == value)
					return;
				if (filterWidget != null) {
					hboxFilter.Remove (filterWidget as Widget);
					(filterWidget as Widget).Destroy ();
					checkShowFilter.Visible = true;
					filterWidget = null;
				}
				if (value != null && value.GetInterface ("IReferenceFilter") == null) {
					throw new NotSupportedException ("FilterClass должен реализовывать интерфейс IReferenceFilter.");
				}
				filterClass = value;
				checkShowFilter.Visible = filterClass != null;
				if (CheckDefaultLoadFilterAtt () || checkShowFilter.Active)
					CreateFilterWidget ();
			}
		}

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null)
					Uow = parentReference.UoW;
			}
			get { return parentReference; }
		}

		private string[] searchFields = new string[]{ "Name" };

		public string[] SearchFields {
			get { return searchFields; }
			set {
				searchFields = value;
				hboxSearch.Visible = searchFields != null && searchFields.Length > 0;
				searchPropCache = null; //Скидываем кеш
			}
		}

		private List<PropertyInfo> searchPropCache;

		protected List<PropertyInfo> SearchPropCache {
			get
			{
				if (searchPropCache != null)
					return searchPropCache;

				if (SearchFields == null)
					return null;

				searchPropCache = new List<PropertyInfo> ();

				foreach (string prop in SearchFields) {
					var propInfo = objectType.GetProperty (prop);
					if (propInfo != null) {
						searchPropCache.Add (propInfo);
					} else
						logger.Error ("У объекта {0} не найдено свойство для поиска {1}.", objectType, prop);
				}

				logger.Debug ("Сформирован кеш свойств для поиска в объекта.");
				return searchPropCache;
			}
		}


		private OrmReferenceMode mode;

		public OrmReferenceMode Mode {
			get { return mode; }
			set {
				mode = value;
				hboxSelect.Visible = (mode == OrmReferenceMode.Select);
			}
		}

		private ReferenceButtonMode buttonMode;

		public ReferenceButtonMode ButtonMode {
			get { return buttonMode; }
			set {
				buttonMode = value;
				buttonAdd.Sensitive = buttonMode.HasFlag (ReferenceButtonMode.CanAdd);
				buttonEdit.Sensitive = buttonMode.HasFlag (ReferenceButtonMode.CanEdit);
				buttonDelete.Sensitive = buttonMode.HasFlag (ReferenceButtonMode.CanDelete);
				Image image = new Image ();
				image.Pixbuf = Stetic.IconLoader.LoadIcon (
					this, 
					buttonMode.HasFlag (ReferenceButtonMode.TreatEditAsOpen) ? "gtk-open" : "gtk-edit", 
					IconSize.Menu);
				buttonEdit.Image = image;
				buttonEdit.Label = buttonMode.HasFlag (ReferenceButtonMode.TreatEditAsOpen) ? "Открыть" : "Изменить";
			}
		}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Справочник";

		public string TabName {
			get { return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
			}
		}

		public OrmReference(System.Type objType) 
			: this(objType, UnitOfWorkFactory.CreateWithoutRoot ())
		{}

		public OrmReference(System.Type objType, IUnitOfWork uow) 
			: this(objType, uow, uow.Session.CreateCriteria (objType))
		{}

		public OrmReference (System.Type objType, IUnitOfWork uow, ICriteria listCriteria)
		{
			this.Build ();
			objectType = objType;
			objectsCriteria = listCriteria;
			Uow = uow;
			ConfigureDlg ();
		}

		public OrmReference (System.Type objType, IUnitOfWork uow, ICriteria listCriteria, string columsMapping)
			: this (objType, uow, listCriteria)
		{
			datatreeviewRef.ColumnMappings = columsMapping;
		}

		public OrmReference (System.Type objType, OrmParentReference parentReference)
		{
			this.Build ();
			objectType = objType;
			ParentReference = parentReference;
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			Mode = OrmReferenceMode.Normal;
			IOrmObjectMapping map = OrmMain.GetObjectDiscription (objectType);
			if (map != null) {
				map.ObjectUpdated += OnRefObjectUpdated;
				datatreeviewRef.ColumnMappings = map.RefColumnMappings;
				if (map.RefSearchFields != null)
					SearchFields = map.RefSearchFields;
				if (map.RefFilterClass != null)
					FilterClass = map.RefFilterClass;
			}
			object[] att = objectType.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0) {
				this.TabName = (att [0] as OrmSubjectAttribute).JournalName;
				ButtonMode = (att [0] as OrmSubjectAttribute).DefaultJournalMode;
			}
			UpdateObjectList ();
			datatreeviewRef.Selection.Changed += OnTreeviewSelectionChanged;
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			//FIXME Проверить может очистка сессии не нужна, если нам ее передали.
			//if (parentReference == null)
			//	Uow.Session.Clear ();
			UpdateObjectList ();
		}

		private void UpdateObjectList ()
		{
			logger.Info ("Получаем таблицу справочника<{0}>...", objectType.Name);
			if (ParentReference == null) {
				if (filterWidget == null)
					filterView = new ObservableFilterListView (objectsCriteria.List ());
				else
					filterView = new ObservableFilterListView (filterWidget.FiltredCriteria.List ());
			} else {
				filterView = new ObservableFilterListView (parentReference.List);
				if (filterWidget != null)
					logger.Warn ("Фильтры(FilterClass) в режиме ParentReference не поддерживаются.");
			}
				
			filterView.IsVisibleInFilter += HandleIsVisibleInFilter;
			filterView.ListChanged += FilterViewChanged;
			datatreeviewRef.ItemsDataSource = filterView;
			if (typeof(ISpecialRowsRender).IsAssignableFrom (objectType)) {
				foreach (TreeViewColumn col in datatreeviewRef.Columns)
					col.SetCellDataFunc (col.Cells [0], new TreeCellDataFunc (RenderCell));
			}
			UpdateSum ();
			logger.Info ("Ok.");
		}

		void OnTreeviewSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewRef.Selection.CountSelectedRows () > 0;
			buttonSelect.Sensitive = selected;
			buttonEdit.Sensitive = ButtonMode.HasFlag(ReferenceButtonMode.CanEdit) && selected;
			buttonDelete.Sensitive = ButtonMode.HasFlag(ReferenceButtonMode.CanDelete) && selected;
		}

		void FilterViewChanged (object aList)
		{
			UpdateSum ();
		}

		bool HandleIsVisibleInFilter (object aObject)
		{
			if (entrySearch.Text == "" || SearchFields.Length == 0)
				return true;
			totalSearchFinished++;
			foreach (var prop in SearchPropCache) {
				string Str = prop.GetValue (aObject, null).ToString ();
				if (Str.IndexOf (entrySearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1) {
					return true;
				}
			}
			return false;
		}

		protected void OnButtonSearchClearClicked (object sender, EventArgs e)
		{
			entrySearch.Text = String.Empty;
		}

		protected void OnEntrySearchChanged (object sender, EventArgs e)
		{
			searchStarted = DateTime.Now;
			totalSearchFinished = 0;
			filterView.Refilter ();
			var delay = DateTime.Now.Subtract (searchStarted);
			logger.Debug ("В поиске обработано {0} элементов за {1} секунд, в среднем по {2} милисекунды на элемент.", 
				totalSearchFinished, delay.TotalSeconds, delay.TotalMilliseconds / totalSearchFinished);
		}

		protected void OnCloseTab ()
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (false));
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			datatreeviewRef.Selection.UnselectAll ();
			if (OrmMain.GetObjectDiscription (objectType).SimpleDialog) {
				OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, objectType, null);
			} else if (parentReference != null) {
				if (TabParent.BeforeCreateNewTab ((object)null, this).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddSlaveTab (this, OrmMain.CreateObjectDialog (objectType, ParentReference));
			} else {
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddTab (OrmMain.CreateObjectDialog (objectType), this);
			}
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			if (OrmMain.GetObjectDiscription (objectType).SimpleDialog) {
				OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, objectType, datatreeviewRef.GetSelectedObjects () [0]);
			} else if (parentReference != null) {
				if (TabParent.BeforeCreateNewTab (datatreeviewRef.GetSelectedObjects () [0], this).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddSlaveTab (this, OrmMain.CreateObjectDialog (objectType, ParentReference, datatreeviewRef.GetSelectedObjects () [0]));
			} else {
				object selected = null;
				if (datatreeviewRef.GetSelectedObjects ().Length > 0)
					selected = datatreeviewRef.GetSelectedObjects () [0];
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				if (selected != null)
					TabParent.AddTab (OrmMain.CreateObjectDialog (objectType, selected), this);
			}
		}

		protected void UpdateSum ()
		{
			labelSum.LabelProp = String.Format ("Количество: {0}", filterView.Count);
			logger.Debug ("Количество обновлено {0}", filterView.Count);
		}

		protected void OnDatatreeviewRefRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			if (Mode == OrmReferenceMode.Select)
				buttonSelect.Click ();
			else if (ButtonMode.HasFlag (ReferenceButtonMode.CanEdit))
				buttonEdit.Click ();
		}

		protected void OnButtonSelectClicked (object sender, EventArgs e)
		{
			if (ObjectSelected != null) {
				ObjectSelected (this, new OrmReferenceObjectSectedEventArgs (
					datatreeviewRef.GetSelectedObjects () [0]
				));
			}
			OnCloseTab ();
		}

		private void CreateFilterWidget ()
		{
			Type[] paramTypes = new Type[]{ typeof(ISession) };	

			System.Reflection.ConstructorInfo ci = filterClass.GetConstructor (paramTypes);
			if (ci == null) {
				InvalidOperationException ex = new InvalidOperationException (
					                               String.Format ("Конструктор в класе фильтра {0} c параметрами({1}) не найден.", filterClass.ToString (), NHibernate.Util.CollectionPrinter.ToString (paramTypes)));
				logger.Error (ex);
				throw ex;
			}
			logger.Debug ("Вызываем конструктор фильтра {0} c параметрами {1}.", filterClass.ToString (), NHibernate.Util.CollectionPrinter.ToString (paramTypes));
			filterWidget = (IReferenceFilter)ci.Invoke (new object[] { Uow });
			filterWidget.BaseCriteria = objectsCriteria;
			filterWidget.Refiltered += OnFilterWidgetRefiltered;
			hboxFilter.Add (filterWidget as Widget);
			(filterWidget as Widget).ShowAll ();
		}

		protected void OnCheckShowFilterToggled (object sender, EventArgs e)
		{
			hboxFilter.Visible = checkShowFilter.Active;
			if (checkShowFilter.Active && filterWidget == null)
				CreateFilterWidget ();
		}

		void OnFilterWidgetRefiltered (object sender, EventArgs e)
		{
			UpdateObjectList ();
		}

		private bool CheckDefaultLoadFilterAtt ()
		{
			if (FilterClass == null)
				return false;
			foreach (var att in FilterClass.GetCustomAttributes (typeof(OrmDefaultIsFilteredAttribute), true)) {
				return (att as OrmDefaultIsFilteredAttribute).DefaultIsFiltered;
			}
			return false;
		}

		private void RenderCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			object o = ((datatreeviewRef.Model as TreeModelAdapter)
				.Implementor as Gtk.DataBindings.MappingsImplementor).NodeFromIter (iter);
			(cell as CellRendererText).Foreground = (o as ISpecialRowsRender).TextColor;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			if (parentReference != null) {
				throw new NotImplementedException();
			} else {
				if (OrmMain.DeleteObject(datatreeviewRef.GetSelectedObjects()[0]))
					UpdateObjectList();
			}
		}
	}
		
	public enum OrmReferenceMode
	{
		Normal,
		Select
	}

	[Flags]
	public enum ReferenceButtonMode
	{
		None = 0,
		CanAdd = 1,
		CanEdit = 2,
		CanDelete = 4,
		CanAll = 7,
		TreatEditAsOpen = 8
	}


	public class OrmReferenceObjectSectedEventArgs : EventArgs
	{
		public object Subject { get; private set; }

		public OrmReferenceObjectSectedEventArgs (object subject)
		{
			Subject = subject;
		}
	}

	public interface ISpecialRowsRender
	{
		string TextColor { get; }
	}
}

