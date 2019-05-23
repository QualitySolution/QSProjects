using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.Tdi;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Dialogs;
using System.Linq;
using QS.DomainModel.Config;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using QS.RepresentationModel.GtkUI;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class RepresentationEntry : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private System.Type subjectType;
		private bool sensitive = true;
		public bool CanEditReference = true;
		public Func<object, string> ObjectDisplayFunc;
		private ListStore completionListStore;
		private bool entryChangedByUser = true;

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;

		public BindingControler<RepresentationEntry> Binding { get; private set; }

		[Browsable(false)]
		public new bool Sensitive {
			get { return sensitive; }
			set {
				if(sensitive == value)
					return;
				sensitive = value;
				UpdateSensitive();
			}
		}

		bool isEditable = true;
		[Browsable(false)]
		public bool IsEditable {
			get { return isEditable; }
			set {
				isEditable = value;
				UpdateSensitive();
			}
		}

		private IRepresentationModel representationModel;

		public IRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set {
				if(representationModel == value)
					return;
				representationModel = value;
				SubjectType = RepresentationModel.EntityType;
				//Включение быстрого выбора
				if(RepresentationModel.CanEntryFastSelect) {
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
				if(subject == value)
					return;
				if(subject is INotifyPropertyChanged) {
					(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
				}
				subject = value;
				if(subject is INotifyPropertyChanged) {
					(subject as INotifyPropertyChanged).PropertyChanged += OnSubjectPropertyChanged;
				}
				UpdateWidget();
				OnChanged();
			}
		}

		public int SubjectId {
			get {
				return DomainHelper.GetId(Subject);
			}
		}

		public TEntity GetSubject<TEntity>()
		{
			return (TEntity)Subject;
		}

		void OnSubjectPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateWidget();
		}

		protected Type SubjectType {
			get { return subjectType; }
			set {
				DomainModel.NotifyChange.NotifyConfiguration.Instance.UnsubscribeAll(this);
				subjectType = value;
				if(subjectType != null) {
					DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribeOnEntity(ExternalEntityChangeEventMethod, subjectType);
				}
			}
		}

		void ExternalEntityChangeEventMethod(DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			object foundUpdatedObject = changeEvents.FirstOrDefault(e => DomainHelper.EqualDomainObjects(e.Entity, Subject));
			if(foundUpdatedObject != null) {
				if(MyEntityDialogExist && !MyEntityDialog.UoW.Session.Contains(foundUpdatedObject))
					MyEntityDialog.UoW.Session.Refresh(Subject);

				UpdateWidget();
				OnChanged();
			}
		}

		private void UpdateWidget()
		{
			buttonViewEntity.Sensitive = CanEditReference && subject != null;
			if(subject == null) {
				InternalSetEntryText(String.Empty);
				return;
			}

			if(ObjectDisplayFunc != null) {
				InternalSetEntryText(ObjectDisplayFunc(Subject));
				return;
			}

			InternalSetEntryText(DomainHelper.GetObjectTilte(Subject));
		}

		private void InternalSetEntryText(string text)
		{
			entryChangedByUser = false;
			entryObject.Text = text;
			entryChangedByUser = true;
		}

		public RepresentationEntry()
		{
			this.Build();
			Binding = new BindingControler<RepresentationEntry>(this, new Expression<Func<RepresentationEntry, object>>[] {
				(w => w.Subject)
			});
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
			if(modelWithParent != null && MyEntityDialogExist) {
				if(MyEntityDialog.UoW.IsNew && MyEntityDialog.EntityObject == modelWithParent.GetParent) {
					if(DialogHelper.SaveBeforeSelectFromChildReference(modelWithParent.GetParent.GetType(), SubjectType)) {
						if(!MyTdiDialog.Save())
							return;
					} else
						return;
				}
			}

			PermissionControlledRepresentationJournal SelectDialog;

			SelectDialog = new PermissionControlledRepresentationJournal(RepresentationModel);
			if(newTabTitle != null)
				SelectDialog.CustomTabName(newTabTitle);
			SelectDialog.Mode = JournalSelectMode.Single;
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void SelectDialog_ObjectSelected(object sender, JournalObjectSelectedEventArgs e)
		{
			SelectSubjectByNode(e.Selected.FirstOrDefault());
			OnChangedByUser();
		}

		protected void OnButtonViewEntityClicked(object sender, EventArgs e)
		{
			IEntityConfig entityConfig = DomainConfiguration.GetEntityConfig(subjectType);
			if(entityConfig.SimpleDialog) {
				EntityEditSimpleDialog.RunSimpleDialog(this.Toplevel as Window, SubjectType, Subject);
				return;
			}

			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null) {
				logger.Warn("Родительская вкладка не найдена.");
				return;
			}

			ITdiTab dlg = entityConfig.CreateDialog(Subject);
			mytab.TabParent.AddTab(dlg, mytab);
		}

		protected virtual void OnChanged()
		{
			Binding.FireChange(new Expression<Func<RepresentationEntry, object>>[] {
				(w => w.Subject)
			});

			if(Changed != null)
				Changed(this, EventArgs.Empty);

		}

		protected virtual void OnChangedByUser()
		{
			if(ChangedByUser != null)
				ChangedByUser(this, EventArgs.Empty);
		}

		[GLib.ConnectBefore]
		protected void OnEntryObjectKeyPressEvent(object o, KeyPressEventArgs args)
		{
			if(RepresentationModel.CanEntryFastSelect)
				return;

			if(args.Event.Key == Gdk.Key.Delete || args.Event.Key == Gdk.Key.BackSpace) {
				Subject = null;
				OnChangedByUser();
			}
		}

		void UpdateSensitive()
		{
			buttonSelectEntity.Sensitive = entryObject.Sensitive = sensitive && IsEditable;
			buttonViewEntity.Sensitive = sensitive && CanEditReference && subject != null;
		}

		protected void SelectSubjectByNode(object node)
		{
			var dlg = DialogHelper.FindParentUowDialog(this);
			var uow = dlg == null ? RepresentationModel.UoW : dlg.UoW;

			Subject = uow.GetById(SubjectType, DomainHelper.GetId(node));
		}

		#region AutoCompletion

		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var title = (string)tree_model.GetValue(iter, (int)СompletionColumn.Title);
			string pattern = String.Format("{0}", Regex.Escape(entryObject.Text));
			(cell as CellRendererText).Markup =
				Regex.Replace(title, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
		}

		bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			var val = completion.Model.GetValue(iter, (int)СompletionColumn.Item);
			return RepresentationModel.SearchFilterNodeFunc(val, key);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			var node = args.Model.GetValue(args.Iter, (int)СompletionColumn.Item);
			SelectSubjectByNode(node);
			OnChangedByUser();
			args.RetVal = true;
		}

		private void fillAutocomplete()
		{
			if(!RepresentationModel.CanEntryFastSelect)
				return;

			logger.Info("Запрос данных для автодополнения...");
			completionListStore = new ListStore(typeof(string), typeof(object));

			if(RepresentationModel.ItemsList == null)
				RepresentationModel.UpdateNodes();

			foreach(var item in RepresentationModel.ItemsList) {
				completionListStore.AppendValues(
					(item as INodeWithEntryFastSelect).EntityTitle,
					item
				);
			}
			entryObject.Completion.Model = completionListStore;
			logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
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
			logger.Debug("EntryReferenceVM Destroyed() called.");
			//Отписываемся от событий.
			DomainModel.NotifyChange.NotifyConfiguration.Instance.UnsubscribeAll(this);
			if(subject is INotifyPropertyChanged) {
				(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
			}
			if(RepresentationModel != null)
				RepresentationModel.Destroy();
			base.OnDestroyed();
		}
	}
}
