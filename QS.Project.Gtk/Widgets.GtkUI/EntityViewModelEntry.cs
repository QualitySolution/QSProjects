using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Gamma.Binding.Core;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	[Obsolete("Используйте новый виджет EntityEntry и новый тип журналов QS.Project.Journal")]
	public partial class EntityViewModelEntry : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private bool entryChangedByUser;

		public BindingControler<EntityViewModelEntry> Binding { get; private set; }
		public bool CanEditReference { get; set; } = true;
		private ListStore completionListStore;


		public event EventHandler Changed;
		public event EventHandler ChangedByUser;

		public EntityViewModelEntry()
		{
			this.Build();
			Binding = new BindingControler<EntityViewModelEntry>(this, new Expression<Func<EntityViewModelEntry, object>>[] {
				(w => w.Subject)
			});
		}

		private bool sensitive = true;
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

		private IEntityAutocompleteSelectorFactory entitySelectorAutocompleteFactory;
		[Browsable(false)]
		public IEntityAutocompleteSelectorFactory EntitySelectorAutocompleteFactory
		{
			get => entitySelectorAutocompleteFactory;
			set => SetEntityAutocompleteSelectorFactory(value);
		}

		private IEntitySelectorFactory entitySelectorFactory;
		[Browsable(false)]
		public IEntitySelectorFactory EntitySelectorFactory
		{
			get => entitySelectorFactory;
			set => SetEntitySelectorFactory(value);
		}

		public void SetEntityAutocompleteSelectorFactory(IEntityAutocompleteSelectorFactory entitySelectorFactory)
		{
			entitySelectorAutocompleteFactory = entitySelectorFactory ?? throw new ArgumentNullException(nameof(entitySelectorFactory));
			this.entitySelectorFactory = entitySelectorAutocompleteFactory;
			SubjectType = entitySelectorFactory.EntityType;
			entryObject.IsEditable = true;
			entryChangedByUser = true;
			ConfigureEntryComplition();
		}

		public void SetEntitySelectorFactory(IEntitySelectorFactory entitySelectorFactory)
		{
			this.entitySelectorFactory = entitySelectorFactory ?? throw new ArgumentNullException(nameof(entitySelectorFactory));
			SubjectType = entitySelectorFactory.EntityType;
			entryObject.IsEditable = false;
			entryChangedByUser = false;
			ConfigureEntryComplition();
		}

		/// <summary>
		/// Determines whether the completions popup window is resized to the width of the entry
		/// </summary>
		public void CompletionPopupSetWidth(bool isResized)
		{ 
			entryObject.Completion.PopupSetWidth = isResized;
		}

		private void ConfigureEntryComplition()
		{
			entryObject.Completion = new EntryCompletion();
			entryObject.Completion.MatchSelected += Completion_MatchSelected;
			entryObject.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText();
			entryObject.Completion.PackStart(cell, true);
			entryObject.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}

		void JournalViewModel_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedNodeFromJournal = e.SelectedNodes.FirstOrDefault();
			if (selectedNodeFromJournal != null)
				SelectSubjectByNode(selectedNodeFromJournal);

			ChangedByUser?.Invoke(sender, e);
		}

		private object subject;
		public object Subject {
			get { return subject; }
			set {
				if(subject == value)
					return;
				if(subject is INotifyPropertyChanged notifyPropertyChangedSubject) {
					notifyPropertyChangedSubject.PropertyChanged -= OnSubjectPropertyChanged;
					notifyPropertyChangedSubject.PropertyChanged += OnSubjectPropertyChanged;
				}
				subject = value;
				if (value == null) {
					selectedNode = null;
				}
				UpdateWidget();
				OnChanged();
			}
		}

		private JournalEntityNodeBase selectedNode;

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

		private Type subjectType;
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
				if(UoW != null && UoW.Session.IsOpen && UoW.Session.Contains(Subject)) {
					UoW.Session.Refresh(Subject);
				}

				UpdateWidget();
				OnChanged();
			}
		}

		private void UpdateWidget()
		{
			buttonViewEntity.Sensitive = CanEditReference && subject != null;
			if(subject == null) {
				InternalSetEntryText(string.Empty);
				return;
			}

			InternalSetEntryText(DomainHelper.GetTitle(Subject));
		}

		private void InternalSetEntryText(string text)
		{
			entryChangedByUser = false;
			entryObject.Text = text;
			entryChangedByUser = true;
		}

		protected void OnButtonSelectEntityClicked(object sender, EventArgs e)
		{
			OpenSelectDialog();
		}

		protected void OnButtonClearClicked(object sender, EventArgs e)
		{
			ClearSubject();
		}

		private void ClearSubject()
		{
			Subject = null;
			OnChangedByUser();
			UpdateWidget();
		}

		IEntitySelector entitySelector;

		/// <summary>
		/// Открывает диалог выбора объекта
		/// </summary>
		public void OpenSelectDialog(string newTabTitle = null)
		{
			if(entitySelector != null) {
				MyTab.TabParent.SwitchOnTab(entitySelector);
				return;
			}

			entitySelector = entitySelectorFactory.CreateSelector();
			entitySelector.OnEntitySelectedResult += JournalViewModel_OnEntitySelectedResult;
			entitySelector.TabClosed += EntitySelector_TabClosed;
			MyTab.TabParent.AddSlaveTab(MyTab, entitySelector);
		}

		void EntitySelector_TabClosed(object sender, EventArgs e)
		{
			entitySelector = null;
		}

		protected void OnButtonViewEntityClicked(object sender, EventArgs e)
		{
			using (var localSelector = entitySelectorFactory?.CreateSelector()) {
				var entityTab = localSelector?.GetTabToOpen(selectedNode);
				if(entityTab != null) {
					MyTab.TabParent.AddTab(entityTab, MyTab);
					return;
				}
			}

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
			Binding.FireChange(new Expression<Func<EntityViewModelEntry, object>>[] {
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

		void UpdateSensitive()
		{
			bool canOpen;
			using (var localSelector = entitySelectorFactory?.CreateSelector()) {
				canOpen = localSelector?.CanOpen(selectedNode) ?? false;
			}

			buttonSelectEntity.Sensitive = entryObject.Sensitive = sensitive && IsEditable;
			buttonViewEntity.Sensitive = sensitive && CanEditReference && subject != null && canOpen;
			buttonClear.Sensitive = sensitive && (subject != null || string.IsNullOrWhiteSpace(entryObject.Text));
		}

		protected void SelectSubjectByNode(object node)
		{
			Subject = UoW.GetById(SubjectType, node.GetId());
			selectedNode = node as JournalEntityNodeBase;
			UpdateSensitive();
		}

		#region AutoCompletion

		bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			return true;
		}

		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var title = (string)tree_model.GetValue(iter, 0);
			string pattern = String.Format("{0}", Regex.Escape(entryObject.Text));
			(cell as CellRendererText).Markup =
				Regex.Replace(title, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			var node = args.Model.GetValue(args.Iter, 1);
			SelectSubjectByNode(node);
			OnChangedByUser();
			args.RetVal = true;
		}

		private IEntityAutocompleteSelector autoCompleteSelector;
		private void FillAutocomplete()
		{
			if(cts.IsCancellationRequested) 
			{
				return;
			}
			
			logger.Info("Запрос данных для автодополнения...");
			completionListStore = new ListStore(typeof(string), typeof(object));
			if(entitySelectorAutocompleteFactory == null) {
				return;
			}
			autoCompleteSelector = entitySelectorAutocompleteFactory.CreateAutocompleteSelector();
			autoCompleteSelector.ListUpdated += OnListUpdated;
			autoCompleteSelector.SearchValues(entryObject.Text);
		}

		private void OnListUpdated(object sender, EventArgs e)
		{
			if(cts.IsCancellationRequested)
				return;

			Gtk.Application.Invoke((senderObject, eventArgs) => {
				if(autoCompleteSelector?.Items == null)
					return;

				foreach(var item in autoCompleteSelector.Items) {
					if(item is JournalNodeBase) {
						completionListStore.AppendValues(
							(item as JournalNodeBase).Title,
							item
						);
					} else if(item is INodeWithEntryFastSelect) {
						completionListStore.AppendValues(
							(item as INodeWithEntryFastSelect).EntityTitle,
							item
						);
					}
				}
				entryObject.Completion.Model = completionListStore;
				entryObject.Completion.PopupCompletion = true;
				logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());

				autoCompleteSelector.Dispose();
			});
		}

		protected void OnEntryObjectFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(string.IsNullOrWhiteSpace(entryObject.Text)) {
				Subject = null;
				OnChangedByUser();
			}
		}

		DateTime lastChangedTime = DateTime.Now;
		bool fillingInProgress = false;
		private CancellationTokenSource cts = new CancellationTokenSource();

		protected void OnEntryObjectChanged(object sender, EventArgs e)
		{
			lastChangedTime = DateTime.Now;
			if(!fillingInProgress && entryChangedByUser) {
				Task.Run(() => {
					fillingInProgress = true;
					try {
						while((DateTime.Now - lastChangedTime).TotalMilliseconds < 200) {
							if(cts.IsCancellationRequested) {
								return;
							}
						}
						Application.Invoke((s, arg) => {
							FillAutocomplete();
						});
					} catch(Exception ex) {
						logger.Error(ex, $"Ошибка во время формирования автодополнения для {nameof(EntityViewModelEntry)}");
					} finally {
						fillingInProgress = false;
					}
				});
			}
		}

		#endregion

		protected override void OnDestroyed()
		{
			logger.Debug("EntityViewModelEntry Destroyed() called.");
			//Отписываемся от событий.
			DomainModel.NotifyChange.NotifyConfiguration.Instance.UnsubscribeAll(this);
			if(subject is INotifyPropertyChanged) {
				(subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
			}
			cts.Cancel();
			Application.Invoke((s, arg) => base.OnDestroyed());
		}
	}
}
