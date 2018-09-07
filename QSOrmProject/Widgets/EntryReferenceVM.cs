using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Gtk;
using NLog;
using QS.Gtk.Widgets;
using QSOrmProject.RepresentationModel;
using QSOrmProject.UpdateNotification;
using QSTDI;

namespace QSOrmProject
{
	[ToolboxItem (true)]
	public partial class EntryReferenceVM : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private System.Type subjectType;
		private bool sensitive = true;
		public bool CanEditReference = true;
		public Func<object, string> ObjectDisplayFunc;
		private ListStore completionListStore;
		private bool entryChangedByUser = true;

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;

		//TODO Реализовать удаление
		//TODO Реализовать удобный выбор через подбор

		[Browsable (false)]
		public new bool Sensitive {
			get { return sensitive; }
			set {
				if (sensitive == value)
					return;
				sensitive = value;
				UpdateSensitive();
			}
		}

		bool isEditable = true;
		[Browsable (false)]
		public bool IsEditable
		{
			get{return isEditable;}
			set	{
				isEditable = value;
				UpdateSensitive();
			}
		}

		private IRepresentationModel representationModel;

		public IRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set { if (representationModel == value)
				return;
				representationModel = value;
				SubjectType = RepresentationModel.ObjectType;
				//Включение быстрого выбора
				if(RepresentationModel.CanEntryFastSelect)
				{
					entryObject.Completion = new EntryCompletion();
					entryObject.Completion.MatchSelected += Completion_MatchSelected;
					entryObject.Completion.MatchFunc = Completion_MatchFunc;
					var cell = new CellRendererText();
					entryObject.Completion.PackStart(cell, true);
					entryObject.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
				}
				entryObject.IsEditable = RepresentationModel.CanEntryFastSelect;
			}
		}

		private object subject;

		public object Subject {
			get { return subject; }
			set {
				if (subject == value)
					return;
				if(subject is INotifyPropertyChanged)
				{
					(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
				}
				subject = value;
				if(subject is INotifyPropertyChanged)
				{
					(subject as INotifyPropertyChanged).PropertyChanged += OnSubjectPropertyChanged;
				}
				UpdateWidget ();
				OnChanged ();
			}
		}

		public int SubjectId
		{
			get
			{
				return DomainHelper.GetId(Subject);
			}
		}

		public TEntity GetSubject<TEntity> ()
		{
			return (TEntity)Subject;
		}

		void OnSubjectPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			UpdateWidget ();
		}

		protected Type SubjectType {
			get { return subjectType; }
			set {
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
					map.ObjectUpdated -= OnExternalObjectUpdated;
				}
				subjectType = value;
				if (subjectType != null) {
					IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
					map.ObjectUpdated += OnExternalObjectUpdated;
				}
			}
		}

		private void OnExternalObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			object foundUpdatedObject = e.UpdatedSubjects.FirstOrDefault (s => DomainHelper.EqualDomainObjects (s, Subject));
			if (foundUpdatedObject != null) {
				IOrmDialog dlg = OrmMain.FindMyDialog (this);

				if (dlg != null && !dlg.UoW.Session.Contains (foundUpdatedObject))
					dlg.UoW.Session.Refresh (Subject);

				UpdateWidget ();
				OnChanged ();
			}
		}

		private void UpdateWidget ()
		{
			buttonViewEntity.Sensitive = CanEditReference && subject != null;
			if (subject == null) {
				InternalSetEntryText(String.Empty);
				return;
			}

			if(ObjectDisplayFunc != null)
			{
				InternalSetEntryText(ObjectDisplayFunc (Subject));
				return;
			}

			InternalSetEntryText(DomainHelper.GetObjectTilte (Subject));
		}

		private void InternalSetEntryText(string text)
		{
			entryChangedByUser = false;
			entryObject.Text = text;
			entryChangedByUser = true;
		}

		public EntryReferenceVM ()
		{
			this.Build ();
		}

		protected void OnButtonSelectEntityClicked(object sender, EventArgs e)
		{
			OpenSelectDialog();
		}

		/// <summary>
		/// Открывает диалог выбора объекта
		/// </summary>
		public void OpenSelectDialog(string newTabTitle = null)
		{ 
			var modelWithParent = RepresentationModel as IRepresentationModelWithParent;
			if(modelWithParent != null) {
				if(MyOrmDialog != null && MyOrmDialog.UoW.IsNew
					&& MyOrmDialog.EntityObject == modelWithParent.GetParent) {
					if(CommonDialogs.SaveBeforeSelectFromChildReference(modelWithParent.GetParent.GetType(), SubjectType)) {
						if(!MyTdiDialog.Save())
							return;
					} else
						return;
				}
			}

			ReferenceRepresentation SelectDialog;

			SelectDialog = new ReferenceRepresentation(RepresentationModel);
			if(newTabTitle != null)
				SelectDialog.TabName = newTabTitle;
			SelectDialog.Mode = OrmReferenceMode.Select;
			if(!CanEditReference)
				SelectDialog.ButtonMode &= ~(ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanDelete);
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void SelectDialog_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			SelectSubjectByNode(e.VMNode);
			OnChangedByUser();
		}

		protected void OnButtonViewEntityClicked (object sender, EventArgs e)
		{
			if (OrmMain.GetObjectDescription (SubjectType).SimpleDialog) {
				OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, SubjectType, Subject);
				return;
			}

			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			ITdiTab dlg = OrmMain.CreateObjectDialog (Subject);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected virtual void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		protected virtual void OnChangedByUser ()
		{
			if (ChangedByUser != null)
				ChangedByUser (this, EventArgs.Empty);
		}

		[GLib.ConnectBefore]
		protected void OnEntryObjectKeyPressEvent (object o, KeyPressEventArgs args)
		{
			if(RepresentationModel.CanEntryFastSelect)
				return;

			if(args.Event.Key == Gdk.Key.Delete || args.Event.Key == Gdk.Key.BackSpace)
			{
				Subject = null;
				OnChangedByUser();
			}
		}

		void UpdateSensitive()
		{
			buttonSelectEntity.Sensitive = entryObject.Sensitive = sensitive && IsEditable;
			buttonViewEntity.Sensitive = sensitive && CanEditReference && subject != null;
		}

		protected void SelectSubjectByNode(object node){
			var dlg = OrmMain.FindMyDialog(this);
			var uow = dlg == null ? RepresentationModel.UoW : dlg.UoW;

			Subject = uow.GetById(SubjectType, DomainHelper.GetId(node));
		}

		#region AutoCompletion

		void OnCellLayoutDataFunc (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var title = (string)tree_model.GetValue (iter, (int)СompletionColumn.Tilte);
			string pattern = String.Format ("{0}", Regex.Escape (entryObject.Text));
			(cell as CellRendererText).Markup = 
				Regex.Replace (title, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			var val = completion.Model.GetValue (iter, (int)СompletionColumn.Item);
			return RepresentationModel.SearchFilterNodeFunc(val, key);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			var node = args.Model.GetValue (args.Iter, (int)СompletionColumn.Item);
			SelectSubjectByNode(node);
			OnChangedByUser();
			args.RetVal = true;
		}

		private void fillAutocomplete ()
		{
			if(!RepresentationModel.CanEntryFastSelect)
				return;
			
			logger.Info ("Запрос данных для автодополнения...");
			completionListStore = new ListStore (typeof(string), typeof(object));

			if(RepresentationModel.ItemsList == null)
				RepresentationModel.UpdateNodes();

			foreach (var item in RepresentationModel.ItemsList) {
				completionListStore.AppendValues (
					(item as INodeWithEntryFastSelect).EntityTitle,
					item
				);
			}
			entryObject.Completion.Model = completionListStore;
			logger.Debug ("Получено {0} строк автодополения...", completionListStore.IterNChildren ());
		}

		protected void OnEntryObjectFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(!RepresentationModel.CanEntryFastSelect)
				return;

			if(string.IsNullOrWhiteSpace(entryObject.Text)) {
				Subject = null;
				OnChangedByUser();
			}
		}

		protected void OnEntryObjectChanged(object sender, EventArgs e)
		{
			if(RepresentationModel == null || !RepresentationModel.CanEntryFastSelect)
				return;
			
			if(entryChangedByUser && completionListStore == null)
				fillAutocomplete();
		}

		#endregion

		protected override void OnDestroyed()
		{
			logger.Debug ("EntryReferenceVM Destroyed() called.");
			//Отписываемся от событий.
			if (subjectType != null) {
				IOrmObjectMapping map = OrmMain.GetObjectDescription (subjectType);
				map.ObjectUpdated -= OnExternalObjectUpdated;
			}
			if (subject is INotifyPropertyChanged) {
				(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
			}
			if (RepresentationModel != null)
				RepresentationModel.Destroy();
			base.OnDestroyed();
		}
	}
}

