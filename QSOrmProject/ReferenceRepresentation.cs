using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NLog;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSTDI;

namespace QSOrmProject
{
	[WidgetWindow(DefaultWidth = 852, DefaultHeight = 600)]
	public partial class ReferenceRepresentation : Gtk.Bin, ITdiJournal, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private System.Type objectType;
		private IRepresentationFilter filterWidget;
		private DateTime searchStarted;
		private IRepresentationModel representationModel;

		public ITdiTabParent TabParent { set; get; }

		public bool FailInitialize { get; protected set;}

		public bool CompareHashName(string hashName)
		{
			return GenerateHashName(RepresentationModel.GetType()) == hashName;
		}

		public static string GenerateHashName(Type presentation)
		{
			if (!typeof(IRepresentationModel).IsAssignableFrom(presentation))
				throw new ArgumentException("Тип должен реализовывать интерфейс IRepresentationModel", "presentation");
			
			return String.Format("View_{0}", presentation.Name);
		}

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
				if(representationModel != null)
					RepresentationModel.ItemsListUpdated -= RepresentationModel_ItemsListUpdated;
				representationModel = value;
				objectType = RepresentationModel.ObjectType;
				RepresentationModel.ItemsListUpdated += RepresentationModel_ItemsListUpdated;
				ormtableview.RepresentationModel = RepresentationModel;
				if (RepresentationModel.RepresentationFilter != null)
					FilterWidget = RepresentationModel.RepresentationFilter;
				hboxSearch.Visible = RepresentationModel.SearchFieldsExist;
			}
		}

		public bool ShowFilter{
			get{ 
				return checkShowFilter.Active;
			}
			set { checkShowFilter.Active = value;
			}
		}

		void RepresentationModel_ItemsListUpdated (object sender, EventArgs e)
		{
			UpdateSum ();
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

		private OrmReferenceMode mode;

		public OrmReferenceMode Mode {
			get { return mode; }
			set {
				mode = value;
				hboxSelect.Visible = (mode == OrmReferenceMode.Select || mode == OrmReferenceMode.MultiSelect);
				ormtableview.Selection.Mode = (mode == OrmReferenceMode.MultiSelect) ? SelectionMode.Multiple : SelectionMode.Single;
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
			buttonAdd.Visible = buttonEdit.Visible = buttonDelete.Visible = objectType != null;
			if(objectType != null)
			{
				object[] att = objectType.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
				if (att.Length > 0) {
					this.TabName = (att [0] as OrmSubjectAttribute).JournalName;
					ButtonMode = (att [0] as OrmSubjectAttribute).DefaultJournalMode;
				}
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

		protected void OnButtonSearchClearClicked (object sender, EventArgs e)
		{
			entrySearch.Text = String.Empty;
		}

		protected void OnEntrySearchChanged (object sender, EventArgs e)
		{
			searchStarted = DateTime.Now;
			RepresentationModel.SearchString = entrySearch.Text;
			var delay = DateTime.Now.Subtract (searchStarted);
			logger.Debug ("Поиск нашел {0} элементов за {1} секунд.", RepresentationModel.ItemsList.Count, delay.TotalSeconds);
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
				TabParent.OpenTab(OrmMain.GenerateDialogHashName(objectType, 0),
					() => OrmMain.CreateObjectDialog (objectType, (RepresentationModel as IRepresentationModelWithParent).GetParent),
					this
				);
			} else {
				TabParent.OpenTab(OrmMain.GenerateDialogHashName(objectType, 0),
					() => OrmMain.CreateObjectDialog (objectType),
					this
				);
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
				int selectedId = ormtableview.GetSelectedId ();
				TabParent.OpenTab(OrmMain.GenerateDialogHashName(objectType, selectedId),
					() => OrmMain.CreateObjectDialog (objectType, selectedId),
					this
				);
			}
		}

		protected void UpdateSum ()
		{
			labelSum.LabelProp = String.Format ("Количество: {0}", RepresentationModel.ItemsList.Count);
			logger.Debug ("Количество обновлено {0}", RepresentationModel.ItemsList.Count);
		}

		protected void OnButtonSelectClicked (object sender, EventArgs e)
		{
			if (ObjectSelected != null) 
			{
				var selected = GetSelectResults();

				ObjectSelected (this, new ReferenceRepresentationSelectedEventArgs (selected));
			}
			OnCloseTab ();
		}

		protected RepresentationSelectResult[] GetSelectResults()
		{
			return ormtableview.GetSelectedObjects ()
				.Select (node => new RepresentationSelectResult (DomainHelper.GetId (node), node))
				.ToArray ();
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
			if (Mode == OrmReferenceMode.Select || Mode == OrmReferenceMode.MultiSelect)
				buttonSelect.Click ();
			else if (ButtonMode.HasFlag (ReferenceButtonMode.CanEdit))
				buttonEdit.Click ();
		}

		[GLib.ConnectBefore]
		protected void OnOrmtableviewButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3 && RepresentationModel.PopupMenuExist)
			{
				var selected = GetSelectResults();
				var menu = RepresentationModel.GetPopupMenu(selected);
				if(menu != null)
				{
					menu.ShowAll();
					menu.Popup();
				}
			}
		}
	}

	public class ReferenceRepresentationSelectedEventArgs : EventArgs
	{
		public int ObjectId {
			get {
				return Selected[0].EntityId;
			}
		}
			
		public object VMNode {
			get {
				return Selected[0].VMNode;
			}
		}

		public RepresentationSelectResult[] Selected { get; private set; }

		public IEnumerable<TNode> GetNodes<TNode> ()
		{
			return Selected.Select (r => r.VMNode).Cast<TNode>();
		}

		public ReferenceRepresentationSelectedEventArgs (RepresentationSelectResult[] selected)
		{
			Selected = selected;
		}
	}

	public class RepresentationSelectResult
	{
		public int EntityId { get; private set; }
		public object VMNode { get; private set; }

		public RepresentationSelectResult(int id, object node)
		{
			EntityId = id;
			VMNode = node;
		}
	}
}

