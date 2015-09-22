using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gtk;
using NLog;
using QSOrmProject.RepresentationModel;
using QSTDI;
using QSOrmProject;
using System.Data.Bindings.Collections;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ReferenceRepresentation : Gtk.Bin, ITdiJournal, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private System.Type objectType;
		private ObservableFilterListView filterView;
		private IRepresentationFilter filterWidget;
		private uint totalSearchFinished;
		private DateTime searchStarted;
		private IRepresentationModel representationModel;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<ReferenceRepresentationSelectedEventArgs> ObjectSelected;

		public IRepresentationFilter FilterWidget {
			get {
				return filterWidget;
			}
			private set {
				if (filterWidget == value)
					return;
				if (filterWidget != null) {
					hboxFilter.Remove (filterWidget as Widget);
					(filterWidget as Widget).Destroy ();
					checkShowFilter.Visible = true;
					filterWidget = null;
				}
				filterWidget = value;
				checkShowFilter.Visible = filterWidget != null;
				hboxFilter.Add (filterWidget as Widget);
				(filterWidget as Widget).ShowAll ();
			}
		}

		protected IRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set {
				if (representationModel == value)
					return;
				representationModel = value;
				objectType = RepresentationModel.ObjectType;
				ormtableview.ColumnMappingConfig = RepresentationModel.TreeViewConfig;
				if (RepresentationModel.RepresentationFilter != null)
					FilterWidget = RepresentationModel.RepresentationFilter;
				SearchFields = RepresentationModel.SearchFields.ToArray ();
				RepresentationModel.ItemsListUpdated += RepresentationModel_ItemsListUpdated;
				RepresentationModel.UpdateNodes ();
			}
		}

		void RepresentationModel_ItemsListUpdated (object sender, EventArgs e)
		{
			logger.Info ("Обновляем таблицу справочника<{0}>...", objectType.Name);
			filterView = new ObservableFilterListView (RepresentationModel.ItemsList);

			filterView.IsVisibleInFilter += HandleIsVisibleInFilter;
			filterView.ListChanged += FilterViewChanged;
			ormtableview.ItemsDataSource = filterView;
			UpdateSum ();
			logger.Info ("Ok.");
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW {
			get {
				return RepresentationModel.UoW;
			}
		}

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

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
					var propInfo = RepresentationModel.NodeType.GetProperty (prop);
					if (propInfo != null) {
						searchPropCache.Add (propInfo);
					} else
						logger.Error ("У объекта {0} не найдено свойство для поиска {1}.", RepresentationModel.NodeType, prop);
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

		public ReferenceRepresentation (IRepresentationModel representation)
		{
			this.Build ();
			RepresentationModel = representation;
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			Mode = OrmReferenceMode.Normal;
			object[] att = objectType.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0) {
				this.TabName = (att [0] as OrmSubjectAttribute).JournalName;
				ButtonMode = (att [0] as OrmSubjectAttribute).DefaultJournalMode;
			}
			ormtableview.Selection.Changed += OnTreeviewSelectionChanged;
		}

		void OnTreeviewSelectionChanged (object sender, EventArgs e)
		{
			bool selected = ormtableview.Selection.CountSelectedRows () > 0;
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
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (false));
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ormtableview.Selection.UnselectAll ();
			var classDiscript = OrmMain.GetObjectDescription (objectType);
			if (classDiscript.SimpleDialog) {
				OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, objectType, null);
			} else if (RepresentationModel is IRepresentationModelWithParent) {
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddTab (
					OrmMain.CreateObjectDialog (objectType, (RepresentationModel as IRepresentationModelWithParent).GetParent), this);
			} else {
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddTab (OrmMain.CreateObjectDialog (objectType), this);
			}
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			var description = OrmMain.GetObjectDescription (objectType);
			if (description == null) {
				throw new NotImplementedException (String.Format ("Не реализован OrmObjectMapping для типа {0}", objectType));
			}
			if (description.SimpleDialog) {
				throw new NotImplementedException ();
				//OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, objectType, datatreeviewRef.GetSelectedObjects () [0]);
			} else {
				var node = ormtableview.GetSelectedObjects () [0];
				int selectedId = DomainHelper.GetId (node);
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
					return;
				if (selectedId > 0)
					TabParent.AddTab (OrmMain.CreateObjectDialog (objectType, selectedId), this);
			}
		}

		protected void UpdateSum ()
		{
			labelSum.LabelProp = String.Format ("Количество: {0}", filterView.Count);
			logger.Debug ("Количество обновлено {0}", filterView.Count);
		}

		protected void OnButtonSelectClicked (object sender, EventArgs e)
		{
			if (ObjectSelected != null) {
				var node = ormtableview.GetSelectedObject ();
				int selectedId = DomainHelper.GetId (node);

				ObjectSelected (this, new ReferenceRepresentationSelectedEventArgs (selectedId, node));
			}
			OnCloseTab ();
		}

		protected void OnCheckShowFilterToggled (object sender, EventArgs e)
		{
			hboxFilter.Visible = checkShowFilter.Active;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var node = ormtableview.GetSelectedObjects () [0];
			int selectedId = DomainHelper.GetId (node);

			if (OrmMain.DeleteObject (objectType, selectedId))
				RepresentationModel.UpdateNodes ();
		}

		protected void OnOrmtableviewRowActivated (object o, RowActivatedArgs args)
		{
			if (Mode == OrmReferenceMode.Select)
				buttonSelect.Click ();
			else if (ButtonMode.HasFlag (ReferenceButtonMode.CanEdit))
				buttonEdit.Click ();
		}
	}

	public class ReferenceRepresentationSelectedEventArgs : EventArgs
	{
		public int ObjectId { get; private set; }

		public object VMNode { get; private set; }

		public ReferenceRepresentationSelectedEventArgs (int id, object node)
		{
			ObjectId = id;
			VMNode = node;
		}
	}
}

