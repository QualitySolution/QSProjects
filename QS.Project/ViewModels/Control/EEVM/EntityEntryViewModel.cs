using System;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;

namespace QS.ViewModels.Control.EEVM
{
	public class EntityEntryViewModel<TEntity> : PropertyChangedBase, IEntityEntryViewModel, IDisposable
		where TEntity: class
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public EntityEntryViewModel(
			IPropertyBinder<TEntity> binder = null,
			IEntitySelector entitySelector = null,
			IEntityDlgOpener dlgOpener = null,
			IEntityAutocompleteSelector<TEntity> autocompleteSelector = null
			)
		{
			if (binder != null)
				this.EntityBinder = binder;
			if (entitySelector != null)
				this.EntitySelector = entitySelector;
			if (dlgOpener != null)
				this.DlgOpener = dlgOpener;
			if (autocompleteSelector != null)
				this.AutocompleteSelector = autocompleteSelector;

			DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribeOnEntity(ExternalEntityChangeEventMethod, typeof(TEntity));
		}

		#region События

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;
		public event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;

		#endregion

		#region Публичные свойства

		private TEntity entity;

		public virtual TEntity Entity {
			get { return entity; }
			protected set {
				if (entity == value)
					return;

				if (entity is INotifyPropertyChanged notifyPropertyOldEntity)
					notifyPropertyOldEntity.PropertyChanged -= Entity_PropertyChanged;

				entity = value;

				if (entity is INotifyPropertyChanged notifyPropertyNewEntity) {
					notifyPropertyNewEntity.PropertyChanged += Entity_PropertyChanged;;
				}

				if(EntityBinder != null)
					EntityBinder.PropertyValue = value;

				OnPropertyChanged();
				OnPropertyChanged(nameof(EntityTitle));
				OnPropertyChanged(nameof(SensetiveCleanButton));
				OnPropertyChanged(nameof(SensetiveViewButton));
				Changed?.Invoke(this, new EntitySelectedEventArgs(value));
			}
		}

		bool isEditable = true;

		public bool IsEditable {
			get { return isEditable; }
			set {
				isEditable = value;
				OnPropertyChanged(nameof(SensetiveSelectButton));
				OnPropertyChanged(nameof(SensetiveCleanButton));
				OnPropertyChanged(nameof(SensetiveAutoCompleteEntry));
			}
		}

		#endregion

		#region Свойства для использования во View

		public string EntityTitle => Entity == null ? null : DomainHelper.GetObjectTilte(Entity);

		public virtual bool SensetiveSelectButton => IsEditable && EntitySelector != null;
		public virtual bool SensetiveCleanButton => IsEditable && Entity != null;
		public virtual bool SensetiveAutoCompleteEntry => IsEditable && AutocompleteSelector != null;
		public virtual bool SensetiveViewButton => DlgOpener != null && Entity != null;
		#endregion

		#region Выбор сущьности основным способом

		private IEntitySelector entitySelector;
		public IEntitySelector EntitySelector {
			get => entitySelector;
			set {
				entitySelector = value;
				EntitySelector.EntitySelected += EntitySelector_EntitySelected;
				OnPropertyChanged(nameof(SensetiveSelectButton));
			}
		}

		/// <summary>
		/// Открывает диалог выбора сущьности
		/// </summary>
		public void OpenSelectDialog()
		{
			entitySelector?.OpenSelector();
		}

		void EntitySelector_EntitySelected(object sender, EntitySelectedEventArgs e)
		{
			Entity = (TEntity)e.Entity;
			ChangedByUser?.Invoke(this, e);
		}

		#endregion

		#region Открытие диалога сущьности

		private IEntityDlgOpener dlgOpener;
		public IEntityDlgOpener DlgOpener {
			get => dlgOpener;
			set {
				dlgOpener = value;
				OnPropertyChanged(nameof(SensetiveViewButton));
			}
		}

		public void OpenViewEntity()
		{
			DlgOpener?.OpenEntityDlg(DomainHelper.GetId(Entity));
		}

		#endregion

		#region AutoCompletion

		public int AutocompleteListSize { get; set; }

		private IEntityAutocompleteSelector<TEntity> autocompleteSelector;
		public IEntityAutocompleteSelector<TEntity> AutocompleteSelector {
			get => autocompleteSelector;
			set {
				autocompleteSelector = value;
				OnPropertyChanged(nameof(SensetiveAutoCompleteEntry));
				autocompleteSelector.AutocompleteLoaded += AutocompleteSelector_AutocompleteLoaded;
			}
		}

		public void AutocompleteTextEdited(string searchText)
		{
			var words = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			AutocompleteSelector?.LoadAutocompletion(words, AutocompleteListSize);
		}

		void AutocompleteSelector_AutocompleteLoaded(object sender, AutocompleteUpdatedEventArgs e)
		{
			AutoCompleteListUpdated?.Invoke(this, e);
		}

		public string GetAutocompleteTitle(object node)
		{
			return AutocompleteSelector.GetTitle(node);
		}

		public void SetEntityByNode(object node)
		{
			Entity = AutocompleteSelector.GetEntityByNode(node);
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Обработка событий

		void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(EntityTitle));
		}

		void ExternalEntityChangeEventMethod(DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			object foundUpdatedObject = changeEvents.FirstOrDefault(e => DomainHelper.EqualDomainObjects(e.Entity, Entity));
			if (foundUpdatedObject != null && entitySelector != null) {
				Entity = (TEntity)entitySelector.RefreshEntity(Entity);
			}
		}

		#endregion

		#region Команды View

		public void CleanEntity()
		{
			Entity = null;
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Entity binding

		IPropertyBinder<TEntity> entityBinder;
		public IPropertyBinder<TEntity> EntityBinder {
			get => entityBinder;
			set {
				if(EntityBinder != null)
					EntityBinder.Changed -= EntityBinder_Changed;
				entityBinder = value;
				if(EntityBinder != null) {
					Entity = entityBinder.PropertyValue;
					entityBinder.Changed += EntityBinder_Changed;
				}
			}
		}

		void EntityBinder_Changed(object sender, EventArgs e)
		{
			Entity = entityBinder.PropertyValue;
		}

		#endregion

		public void Dispose()
		{
			DomainModel.NotifyChange.NotifyConfiguration.Instance.UnsubscribeAll(this);
			if (entity is INotifyPropertyChanged notifyPropertyChanged) {
				notifyPropertyChanged.PropertyChanged -= Entity_PropertyChanged;
			}

			if (EntitySelector is IDisposable esd)
				esd.Dispose();
			if (AutocompleteSelector is IDisposable asd)
				asd.Dispose();
			if (EntityBinder is IDisposable ebd)
				ebd.Dispose();
			if (DlgOpener is IDisposable dod)
				dod.Dispose();
		}
	}
}
