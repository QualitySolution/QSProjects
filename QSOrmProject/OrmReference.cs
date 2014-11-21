using System;
using System.Collections;
using System.Data.Bindings;
using System.Data.Bindings.Collections;
using NHibernate;
using QSTDI;
using NLog;
using Gtk;
using Gtk.DataBindings;

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
			OrmObjectMaping map = OrmMain.GetObjectDiscription(objectType);
			if (map != null)
			{
				map.ObjectUpdated += OnRefObjectUpdated;
				datatreeviewRef.ColumnMappings = map.RefColumnMappings;
			}
			object[] att = objectType.GetCustomAttributes(typeof(OrmSubjectAttributes), true);
			if (att.Length > 0)
				this.TabName = (att[0] as OrmSubjectAttributes).JournalName;
			UpdateObjectList();
			datatreeviewRef.Selection.Changed += OnTreeviewSelectionChanged;
			datatreeviewRef.ItemsDataSource = filterView;
			UpdateSum();
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			//FIXME Проверить может очистка сессии не нужна, если нам ее передали.
			if(parentReference != null)
				session.Clear();
			UpdateObjectList();
		}

		private void UpdateObjectList()
		{
			if(ParentReference == null)
				filterView = new ObservableFilterListView (objectsCriteria.List());
			else
				filterView = new ObservableFilterListView (parentReference.List);
			filterView.IsVisibleInFilter += HandleIsVisibleInFilter;
			filterView.ListChanged += FilterViewChanged;
			datatreeviewRef.ItemsDataSource = filterView;
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
			if(entrySearch.Text == "")
				return true;
			foreach(string prop in SearchFields)
			{
				string Str = objectType.GetProperty(prop).GetValue(aObject, null).ToString();
				if (Str.IndexOf (entrySearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1)
					return true;
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
				if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag(TdiBeforeCreateResultFlag.Canceled))
					return;
				TabParent.AddTab (OrmMain.CreateObjectDialog (objectType, datatreeviewRef.GetSelectedObjects()[0]), this);
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

