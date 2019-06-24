using System;
using QS.Dialog.Gtk;
using QS.Project.Journal;
using System.Linq;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using NLog;
using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.DomainModel.Config;
using QS.Project.Dialogs.GtkUI;
using Gtk;
using QS.Tdi;
using System.Text.RegularExpressions;
using QS.Project.Journal.EntitySelector;
using System.Threading.Tasks;
using System.Threading;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class EntityViewModelEntry : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private bool entryChangedByUser = true;

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

		private IEntitySelector entitySelector;
		private IEntitySelectorFactory entitySelectorFactory;

		public void SetEntitySelectorFactory(IEntitySelectorFactory entitySelectorFactory)
		{
			this.entitySelectorFactory = entitySelectorFactory ?? throw new ArgumentNullException(nameof(entitySelectorFactory));
			CreateSelector();
			entryObject.IsEditable = true;
			ConfigureEntryComplition();
		}

		private void CreateSelector()
		{
			entitySelector = entitySelectorFactory.CreateSelector();
			SubjectType = entitySelector.EntityType;
			entitySelector.OnEntitySelectedResult += JournalViewModel_OnEntitySelectedResult;
			entitySelector.CloseTab += EntitySelector_CloseTab;
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

		void EntitySelector_CloseTab(object sender, TdiTabCloseEventArgs e)
		{
			entitySelector.CloseTab -= EntitySelector_CloseTab;
			entitySelector = null;
		}


		void JournalViewModel_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedNode = e.SelectedNodes.FirstOrDefault();
			if(selectedNode == null) {
				return;
			}
			Subject = UoW.GetById(SubjectType, selectedNode.Id);
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

			InternalSetEntryText(DomainHelper.GetObjectTilte(Subject));
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

		[GLib.ConnectBefore]
		protected void OnEntryObjectKeyPressEvent(object o, KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Delete || args.Event.Key == Gdk.Key.BackSpace) {
				ClearSubject();
			}
		}

		private void ClearSubject()
		{
			Subject = null;
			OnChangedByUser();
			UpdateWidget();
		}

		/// <summary>
		/// Открывает диалог выбора объекта
		/// </summary>
		public void OpenSelectDialog(string newTabTitle = null)
		{
			if(entitySelector == null || !entitySelector.IsActive) {
				CreateSelector();
			}
			MyTab.TabParent.AddSlaveTab(MyTab, entitySelector);
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
			buttonSelectEntity.Sensitive = entryObject.Sensitive = sensitive && IsEditable;
			buttonViewEntity.Sensitive = sensitive && CanEditReference && subject != null;
			buttonClear.Sensitive = sensitive && (subject != null || string.IsNullOrWhiteSpace(entryObject.Text));
		}

		protected void SelectSubjectByNode(object node)
		{
			Subject = UoW.GetById(SubjectType, DomainHelper.GetId(node));
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

		private void FillAutocomplete()
		{
			logger.Info("Запрос данных для автодополнения...");
			completionListStore = new ListStore(typeof(string), typeof(object));

			using(var autoCompleteSelector = entitySelectorFactory.CreateSelector()) {
				autoCompleteSelector.Search.SearchValues = new string[] { entryObject.Text };

				foreach(var item in autoCompleteSelector.Items) {
					completionListStore.AppendValues(
						(item as JournalNodeBase).Title,
						item
					);
				}
			}

			entryObject.Completion.Model = completionListStore;
			entryObject.Completion.PopupCompletion = true;
			logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
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
			if(!fillingInProgress) {
				Task.Run(() => {
					fillingInProgress = true;
					try {
						while((DateTime.Now - lastChangedTime).TotalMilliseconds < 200) {
							if(cts.IsCancellationRequested) {
								return;
							}
						}
						Application.Invoke((s, arg) => {
							if(entryChangedByUser) {
								FillAutocomplete();
							}
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
			if(entitySelector != null)
				entitySelector.Dispose();
			base.OnDestroyed();
		}
	}
}
