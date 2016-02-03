using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections;
using System.Reflection;
using Gtk;
using NHibernate;
using NLog;
using QSOrmProject.UpdateNotification;
using QSTDI;
using NHibernate.Criterion;
using System.Linq;
using QSProjectsLib;

namespace QSOrmProject
{
	[WidgetWindow (DefaultWidth = 852, DefaultHeight = 600)]
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
		private bool inUpdating;

		int number = -1;

		public ITdiTabParent TabParent { set; get; }

		public bool FailInitialize { get; protected set; }

		public event EventHandler<OrmReferenceObjectSectedEventArgs> ObjectSelected;

		public IUnitOfWork Uow {
			get {
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
			get {
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
				if (value == OrmReferenceMode.MultiSelect)
					throw new NotImplementedException ();
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
				OnTreeviewSelectionChanged (this, EventArgs.Empty);
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

		public OrmReference (System.Type objType)
			: this (objType, UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public OrmReference (System.Type objType, IUnitOfWork uow)
			: this (objType, uow, uow.Session.CreateCriteria (objType))
		{
		}

		public OrmReference (IUnitOfWork uow, QueryOver query)
			: this (query.DetachedCriteria.GetRootEntityTypeIfAvailable (), uow, query.DetachedCriteria.GetExecutableCriteria (uow.Session))
		{
		}

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

		void ConfigureDlg ()
		{
			OrmMain.Count++;
			number = OrmMain.Count;
			logger.Debug ("Открытие {0}", number);
			Mode = OrmReferenceMode.Normal;
			IOrmObjectMapping map = OrmMain.GetObjectDescription (objectType);
			if (map != null) {
				map.ObjectUpdated += OnRefObjectUpdated;
				datatreeviewRef.ColumnMappings = map.RefColumnMappings;
				if (map.RefSearchFields != null)
					SearchFields = map.RefSearchFields;
				if (map.RefFilterClass != null)
					FilterClass = map.RefFilterClass;
			} else
				throw new InvalidOperationException (String.Format ("Для использования диалога, класс {0} должен быть добавлен в мапинг OrmMain.ClassMappingList.", objectType));
			object[] att = objectType.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0) {
				this.TabName = (att [0] as OrmSubjectAttribute).JournalName;
				ButtonMode = (att [0] as OrmSubjectAttribute).DefaultJournalMode;
			}
			if (!String.IsNullOrWhiteSpace (map.EditPermisionName)) {
				if (!QSMain.User.Permissions [map.EditPermisionName]) {
					ButtonMode &= ~ReferenceButtonMode.CanAll;
				}
			}
			UpdateObjectList ();
			datatreeviewRef.Selection.Changed += OnTreeviewSelectionChanged;
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			//Обновляем загруженные сущности так как в методе UpdateObjectList обновится только список(добавятся новые), но уже загруженные возьмутся из кеша.
			foreach (var entity in e.UpdatedSubjects.OfType<IDomainObject> ()) {
				var curEntity = filterView.OfType<IDomainObject> ().FirstOrDefault (o => o.Id == entity.Id);
				if (curEntity != null)
					Uow.Session.Refresh (curEntity);
			}
			UpdateObjectList ();
		}

		private void UpdateObjectList ()
		{
			if (inUpdating)
				return;
			logger.Debug ("Обновление в диалоге {0}", number);
			logger.Info ("Получаем таблицу справочника<{0}>...", objectType.Name);
			inUpdating = true;
			ICriteria baseCriteria = filterWidget == null ? objectsCriteria : filterWidget.FiltredCriteria;

			var map = OrmMain.GetObjectDescription (objectType);
			if (map.SimpleDialog)
				baseCriteria = baseCriteria.AddOrder (Order.Asc ("Name"));
			else if (map.TableView != null && map.TableView.OrderBy.Count > 0) {
				foreach (var ord in map.TableView.OrderBy) {
					if (ord.Direction == QSOrmProject.DomainMapping.OrderDirection.Desc)
						baseCriteria = baseCriteria.AddOrder (Order.Desc (ord.PropertyName));
					else
						baseCriteria = baseCriteria.AddOrder (Order.Asc (ord.PropertyName));
				}
			}
			filterView = new ObservableFilterListView (baseCriteria.List ());
				
			filterView.IsVisibleInFilter += HandleIsVisibleInFilter;
			filterView.ListChanged += FilterViewChanged;
			datatreeviewRef.ItemsDataSource = filterView;
			if (typeof(ISpecialRowsRender).IsAssignableFrom (objectType)) {
				foreach (TreeViewColumn col in datatreeviewRef.Columns)
					col.SetCellDataFunc (col.Cells [0], new TreeCellDataFunc (RenderCell));
			}
			UpdateSum ();
			inUpdating = false;
			logger.Info ("Ok.");
		}

		void OnTreeviewSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewRef.Selection.CountSelectedRows () > 0;
			buttonSelect.Sensitive = selected;
			buttonEdit.Sensitive = ButtonMode.HasFlag (ReferenceButtonMode.CanEdit) && selected;
			buttonDelete.Sensitive = ButtonMode.HasFlag (ReferenceButtonMode.CanDelete) && selected;
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
			logger.Debug ("Закрытие диалога {0}", number);
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (false));
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			datatreeviewRef.Selection.UnselectAll ();
			if (OrmMain.GetObjectDescription (objectType).SimpleDialog) {
				SelectObject (OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, objectType, null));
			} else {
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				var NewItemDlg = OrmMain.CreateObjectDialog (objectType);
				NewItemDlg.EntitySaved += NewItemDlg_EntitySaved;
				TabParent.AddTab (NewItemDlg, this);
			}
		}

		private void SelectObject (object item)
		{
			if (item == null)
				return;

			var ownItem = filterView.OfType<IDomainObject> ().FirstOrDefault (i => i.Id == DomainHelper.GetId (item));
			datatreeviewRef.SelectObject (ownItem);
		}

		void NewItemDlg_EntitySaved (object sender, EntitySavedEventArgs e)
		{
			if (e.TabClosed)
				SelectObject (e.Entity);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			if (OrmMain.GetObjectDescription (objectType).SimpleDialog) {
				OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, objectType, datatreeviewRef.GetSelectedObjects () [0]);
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
			Type[] paramTypes = new Type[]{ typeof(IUnitOfWork) };	

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

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (OrmMain.DeleteObject (datatreeviewRef.GetSelectedObjects () [0]))
				UpdateObjectList ();
		}

		public override void Destroy ()
		{
			logger.Debug ("OrmReference #{0} Destroy() called.", number);
			IOrmObjectMapping map = OrmMain.GetObjectDescription (objectType);
			if (map != null) {
				map.ObjectUpdated -= OnRefObjectUpdated;
			}
			base.Destroy ();
		}

		[GLib.ConnectBefore]
		protected void OnDatatreeviewRefButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			IOrmObjectMapping map = OrmMain.GetObjectDescription (objectType);

			if(args.Event.Button == 3 && map.PopupMenuExist)
			{
				var selected = datatreeviewRef.GetSelectedObjects();
				var menu = map.GetPopupMenu(selected);
				if(menu != null)
				{
					menu.ShowAll();
					menu.Popup();
				}
			}
		}
	}

	public enum OrmReferenceMode
	{
		Normal,
		Select,
		MultiSelect
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

