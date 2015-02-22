using System;
using System.Collections;
using System.Data.Bindings;
using System.Data.Bindings.Collections;
using NHibernate;
using QSTDI;
using NLog;
using Gtk;
using Gtk.DataBindings;
using QSProjectsLib;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrmReference : Gtk.Bin, ITdiJournal
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private ICriteria objectsCriteria;
		private System.Type objectType;
		private ObservableFilterListView filterView;
		private IReferenceFilter filterWidget;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiOpenObjDialogEventArgs> OpenObjDialog;
		public event EventHandler<TdiOpenObjDialogEventArgs> DeleteObj;
		public event EventHandler<OrmReferenceObjectSectedEventArgs> ObjectSelected;

		private bool canEdit = true;
		public bool CanEdit
		{
			get
			{
				return canEdit;
			}
			set
			{
				canEdit = value;
				buttonAdd.Sensitive = canEdit;
			}
		}

		public ISession Session
		{
			get
			{
				if (session == null)
					session = OrmMain.Sessions.OpenSession();
				return session;
			}
			set
			{
				session = value;
			}
		}

		System.Type filterClass;
		public System.Type FilterClass {
			get {
				return filterClass;
			}
			set {
				if (filterClass == value)
					return;
				if(filterWidget != null)
				{
					hboxFilter.Remove (filterWidget as Widget);
					(filterWidget as Widget).Destroy();
					checkShowFilter.Visible = true;
					filterWidget = null;
				}
				if(value != null && value.GetInterface ("IReferenceFilter") == null)
				{
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
					Session = parentReference.Session;
			}
			get {
				return parentReference;
			}
		}

		private string[] searchFields = new string[]{"Name"};
		public string[] SearchFields
		{
			get
			{
				return searchFields;
			}
			set
			{
				searchFields = value;
				hboxSearch.Visible = searchFields != null && searchFields.Length > 0;
			}
		}

		private OrmReferenceMode mode;
		public OrmReferenceMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = value;
				hboxSelect.Visible = (mode == OrmReferenceMode.Select);
			}
		}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Справочник";
		public string TabName
		{
			get{return _tabName;}
			set{
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}

		}
			
		public OrmReference(System.Type objType, ISession listSession, ICriteria listCriteria)
		{
			this.Build();
			objectType = objType;
			objectsCriteria = listCriteria;
			Session = listSession;
			ConfigureDlg ();
		}

		public OrmReference(System.Type objType, ISession listSession, ICriteria listCriteria, string columsMapping) 
			: this(objType, listSession, listCriteria)
		{
			datatreeviewRef.ColumnMappings = columsMapping;
		}

		public OrmReference(System.Type objType, OrmParentReference parentReference) 
		{
			this.Build();
			objectType = objType;
			ParentReference = parentReference;
			ConfigureDlg ();
		}

		void ConfigureDlg()
		{
			Mode = OrmReferenceMode.Normal;
			OrmObjectMapping map = OrmMain.GetObjectDiscription(objectType);
			if (map != null)
			{
				map.ObjectUpdated += OnRefObjectUpdated;
				datatreeviewRef.ColumnMappings = map.RefColumnMappings;
				if (map.RefSearchFields != null)
					SearchFields = map.RefSearchFields;
				if (map.RefFilterClass != null)
					FilterClass = map.RefFilterClass;
			}
			object[] att = objectType.GetCustomAttributes(typeof(OrmSubjectAttribute), true);
			if (att.Length > 0)
				this.TabName = (att[0] as OrmSubjectAttribute).JournalName;
			UpdateObjectList();
			datatreeviewRef.Selection.Changed += OnTreeviewSelectionChanged;
			datatreeviewRef.ItemsDataSource = filterView;
			UpdateSum();
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			//FIXME Проверить может очистка сессии не нужна, если нам ее передали.
			if(parentReference == null)
				session.Clear();
			UpdateObjectList();
		}

		private void UpdateObjectList()
		{
			logger.Info ("Получаем таблицу справочника<{0}>...", objectType.Name);
			if(ParentReference == null)
			{
				if(filterWidget == null)
					filterView = new ObservableFilterListView (objectsCriteria.List());
				else
					filterView = new ObservableFilterListView (filterWidget.FiltredCriteria.List());
			}
			else
			{
				filterView = new ObservableFilterListView (parentReference.List);
				if (filterWidget != null)
					logger.Warn ("Фильтры(FilterClass) в режиме ParentReference не поддерживаются.");
			}
				
			filterView.IsVisibleInFilter += HandleIsVisibleInFilter;
			filterView.ListChanged += FilterViewChanged;
			datatreeviewRef.ItemsDataSource = filterView;
			UpdateSum ();
			logger.Info ("Ok.");
		}

		void OnTreeviewSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewRef.Selection.CountSelectedRows() > 0;
			buttonSelect.Sensitive = selected;
			buttonEdit.Sensitive = buttonDelete.Sensitive = canEdit && selected;
		}

		void FilterViewChanged (object aList)
		{
			UpdateSum();
		}

		bool HandleIsVisibleInFilter (object aObject)
		{
			if(entrySearch.Text == "" || SearchFields.Length == 0)
				return true;
			foreach (string prop in SearchFields) {
				if (objectType.GetProperty (prop) != null) {
					string Str = objectType.GetProperty (prop).GetValue (aObject, null).ToString ();
					if (Str.IndexOf (entrySearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1)
						return true;
				} else
					logger.Error ("У объекта {0} не найден столбец поиска {1}", aObject.ToString (), prop);
			}
			return false;
		}

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			entrySearch.Text = String.Empty;
		}

		protected void OnEntrySearchChanged(object sender, EventArgs e)
		{
			filterView.Refilter();
		}

		public override void Destroy()
		{
			if(session != null && ParentReference == null)
				session.Close();
			base.Destroy();
		}

		protected void OnCloseTab()
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(false));
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			datatreeviewRef.Selection.UnselectAll();
			if (OrmMain.GetObjectDiscription(objectType).SimpleDialog)
			{
				OrmSimpleDialog.RunSimpleDialog(this.Toplevel as Window, objectType, null);
			}
			else if (parentReference != null)
			{
				if (TabParent.BeforeCreateNewTab ((object)null, this).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddSlaveTab(this, OrmMain.CreateObjectDialog (objectType, ParentReference));
			}
			else
			{
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag(TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddTab (OrmMain.CreateObjectDialog (objectType), this);
			}
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			if(OrmMain.GetObjectDiscription(objectType).SimpleDialog)
			{
				OrmSimpleDialog.RunSimpleDialog(this.Toplevel as Window, objectType, datatreeviewRef.GetSelectedObjects()[0]);
			}
			else if (parentReference != null)
			{
				if (TabParent.BeforeCreateNewTab (datatreeviewRef.GetSelectedObjects()[0], this).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddSlaveTab(this, OrmMain.CreateObjectDialog (objectType, ParentReference, datatreeviewRef.GetSelectedObjects()[0]));
			}
			else
			{
				object selected = null;
				if (datatreeviewRef.GetSelectedObjects().Length > 0)
					selected = datatreeviewRef.GetSelectedObjects () [0];
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag(TdiBeforeCreateResultFlag.Canceled))
					return;
				if (selected != null)
					TabParent.AddTab (OrmMain.CreateObjectDialog (objectType, selected), this);
			}
		}

		protected void UpdateSum()
		{
			labelSum.LabelProp = String.Format("Количество: {0}", filterView.Count);
			logger.Debug("Количество обновлено {0}", filterView.Count);
		}

		protected void OnDatatreeviewRefRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if (Mode == OrmReferenceMode.Select)
				buttonSelect.Click();
			else
				buttonEdit.Click();
		}

		protected void OnButtonSelectClicked(object sender, EventArgs e)
		{
			if(ObjectSelected != null)
			{
				ObjectSelected(this, new OrmReferenceObjectSectedEventArgs(
					datatreeviewRef.GetSelectedObjects()[0]
				));
			}
			OnCloseTab();
		}

		private void CreateFilterWidget()
		{
			Type[] paramTypes = new Type[]{typeof(ISession)};	

			System.Reflection.ConstructorInfo ci = filterClass.GetConstructor(paramTypes);
			if(ci == null)
			{
				InvalidOperationException ex = new InvalidOperationException(
					String.Format("Конструктор в класе фильтра {0} c параметрами({1}) не найден.", filterClass.ToString(), NHibernate.Util.CollectionPrinter.ToString (paramTypes)));
				logger.Error(ex);
				throw ex;
			}
			logger.Debug ("Вызываем конструктор фильтра {0} c параметрами {1}.", filterClass.ToString(), NHibernate.Util.CollectionPrinter.ToString (paramTypes));
			filterWidget = (IReferenceFilter)ci.Invoke(new object[] {Session});
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

		private bool CheckDefaultLoadFilterAtt()
		{
			if (FilterClass == null)
				return false;
			foreach(var att in FilterClass.GetCustomAttributes (typeof(OrmDefaultIsFiltered), true))
			{
				return (att as OrmDefaultIsFiltered).DefaultIsFiltered;
			}
			return false;
		}
	}

	public enum OrmReferenceMode {
		Normal,
		Select
	}

	public class OrmReferenceObjectSectedEventArgs : EventArgs
	{
		public object Subject { get; private set; }

		public OrmReferenceObjectSectedEventArgs(object subject)
		{
			Subject= subject;
		}
	}

}

