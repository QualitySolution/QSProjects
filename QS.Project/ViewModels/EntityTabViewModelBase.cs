using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Services;
using QS.Utilities.Text;

namespace QS.ViewModels
{
	public abstract class EntityTabViewModelBase<TEntity> : DialogTabViewModelBase, IEntityDialog<TEntity>, IEntityDialog
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
	{
		#region IEntityDialog implementation

		public override IUnitOfWork UoW => UoWGeneric;

		public IUnitOfWorkGeneric<TEntity> UoWGeneric { get; set; }

		public TEntity Entity => UoWGeneric.Root;

		public object EntityObject => UoWGeneric.Root;

		#endregion

		private string tabName = string.Empty;
		public override string TabName {
			get {
				if(!string.IsNullOrWhiteSpace(tabName))
					return tabName;
				if(UoW != null && UoW.RootObject != null) {
					AppellativeAttribute subAtt = typeof(TEntity).GetCustomAttribute<AppellativeAttribute>(true);

					if(UoW.IsNew) {
						if(subAtt != null && !string.IsNullOrWhiteSpace(subAtt.Nominative)) {
							switch(subAtt.Gender) {
								case GrammaticalGender.Masculine:
									return "Новый " + subAtt.Nominative;
								case GrammaticalGender.Feminine:
									return "Новая " + subAtt.Nominative;
								case GrammaticalGender.Neuter:
									return "Новое " + subAtt.Nominative;
								default:
									return "Новый(ая) " + subAtt.Nominative;
							}
						}
					} else {
						var notifySubject = UoW.RootObject as INotifyPropertyChanged;

						var prop = UoW.RootObject.GetType().GetProperty("Title");
						if(prop != null) {
							if(notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_TitlePropertyChanged;
								notifySubject.PropertyChanged += Subject_TitlePropertyChanged;
							}
							var title = prop.GetValue(UoW.RootObject, null)?.ToString();
							if(!String.IsNullOrWhiteSpace(title))
								return title;
						}

						prop = UoW.RootObject.GetType().GetProperty("Name");
						if(prop != null) {
							if(notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_NamePropertyChanged;
								notifySubject.PropertyChanged += Subject_NamePropertyChanged;
							}
							var name = prop.GetValue(UoW.RootObject, null)?.ToString();
							if(!String.IsNullOrWhiteSpace(name))
								return name;
						}

						if(subAtt != null && !string.IsNullOrWhiteSpace(subAtt.Nominative))
							return subAtt.Nominative.StringToTitleCase();
					}
					return UoW.RootObject.ToString();
				}
				return string.Empty;
			}
			set {
				if(tabName == value)
					return;
				tabName = value;
				OnTabNameChanged();
			}
		}

		protected ValidationContext ValidationContext { get; set; }
		protected IUserService UserService { get; private set; }
		protected IPermissionResult PermissionResult { get; private set; }

		public UserBase CurrentUser { get; set; }
		protected ICommonServices CommonServices { get; }

		[Obsolete("[Только для Водовоза]Этот конструктор реализован для совместимости со старыми вызовами, используйте конструктор который принимает IUnitOfWorkFactory в качестве внешней зависимости.")]
		protected EntityTabViewModelBase(IEntityUoWBuilder ctorParam, ICommonServices commonServices) 
			: this(ctorParam, QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory, commonServices)
		{ }

		//NavigationManager navigation = null - чтобы не переделывать классов в Водовозе, где будет использоваться передадут.
		protected EntityTabViewModelBase(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null)
			: base(unitOfWorkFactory, (commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService, navigation)
		{
			CommonServices = commonServices;

			if(uowBuilder == null) {
				throw new ArgumentNullException(nameof(uowBuilder));
			}
			UoWGeneric = uowBuilder.CreateUoW<TEntity>(unitOfWorkFactory);

			Initialize();
		}

		private void Initialize()
		{
			UserService = CommonServices.UserService;

			ValidationContext = new ValidationContext(Entity);
			Entity.PropertyChanged += Entity_PropertyChanged;
			CurrentUser = UserService.GetCurrentUser();
			PermissionResult = CommonServices.PermissionService.ValidateUserPermission(typeof(TEntity), UserService.CurrentUserId);

			if(!PermissionResult.CanRead) {
				AbortOpening(PermissionsSettings.GetEntityReadValidateResult(typeof(TEntity)));
			}
		}

		protected virtual bool BeforeSave()
		{
			return true;
		}

		public override bool Save(bool close)
		{
			if(!Validate()) {
				return false;
			}
            if(!BeforeSave())
            {
				return false;
            }
			bool result = base.Save(close);
			AfterSave();
			return result;
		}

		protected virtual bool SaveBeforeContinue()
		{
			var docName = DomainHelper.GetSubjectNames(Entity)?.Nominative?.StringToTitleCase();

			if(string.IsNullOrWhiteSpace(docName))
				docName = "этот документ.";
			else
				docName = string.Concat("\"", docName, "\"");

			var response = AskQuestion(
				string.Format("Перед продолжением необходимо сохранить {0}\nПродолжить?", docName),
				"Сохранить?"
			);
			if(!response)
				return false;
			return Save();
		}

		protected virtual void AfterSave()
		{
		}

		protected virtual bool BeforeValidation() => true;

		protected bool Validate() => BeforeValidation() && CommonServices.ValidationService.Validate(Entity, ValidationContext);

		void Subject_NamePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "Name")
				OnTabNameChanged();
		}

		void Subject_TitlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "Title")
				OnTabNameChanged();
		}

		public override bool CompareHashName(string hashName)
		{
			if(Entity == null || UoWGeneric == null || UoWGeneric.IsNew) {
				return false;
			}
			return GenerateHashName(Entity.Id) == hashName;
		}

		public static string GenerateHashName(int id)
		{
			return DomainHelper.GenerateDialogHashName(typeof(TEntity), id);
		}

		public static string GenerateHashName(TEntity entity)
		{
			return DomainHelper.GenerateDialogHashName(typeof(TEntity), entity.Id);
		}

		public static string GenerateHashName()
		{
			return DomainHelper.GenerateDialogHashName(typeof(TEntity), 0);
		}

		protected override void OnTabNameChanged()
		{
			if(UoW?.ActionTitle != null) {
				UoWGeneric.ActionTitle.UserActionTitle = $"Диалог '{TabName}'";
			}

			base.OnTabNameChanged();
		}

		#region Подписки на изменения свойств

		private Dictionary<string, IList<string>> propertyTriggerRelations = new Dictionary<string, IList<string>>();
		private Dictionary<string, List<Action>> propertyOnChangeActions = new Dictionary<string, List<Action>>();
		private List<Action> onAnyPropertyChangedActions = new List<Action>();

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(propertyTriggerRelations.ContainsKey(e.PropertyName)) {
				foreach(var relatedPropertyName in propertyTriggerRelations[e.PropertyName]) {
					OnPropertyChanged(relatedPropertyName);
				}
			}

			if(propertyOnChangeActions.ContainsKey(e.PropertyName)) {
				foreach(var action in propertyOnChangeActions[e.PropertyName]) {
					action.Invoke();
				}
			}

			foreach(var onChangeAction in onAnyPropertyChangedActions) {
				onChangeAction.Invoke();
			}
		}

		/// <summary>
		/// Устанавливает зависимость свойства сущности к свойствам модели представления.
		/// Если произойдет изменение указанного свойства сущности, то вызовутся изменения всех связанных свойств модели представления
		/// </summary>
		protected void SetPropertyChangeRelation(Expression<Func<TEntity, object>> entityTriggeredProperty, params Expression<Func<object>>[] vmChangingPropertiesExpressions)
		{
			IList<string> vmChangingProperties = new List<string>();
			string entityPropertyName = Entity.GetPropertyName(entityTriggeredProperty);
			if(propertyTriggerRelations.ContainsKey(entityPropertyName)) {
				vmChangingProperties = propertyTriggerRelations[entityPropertyName];
			} else {
				propertyTriggerRelations.Add(entityPropertyName, vmChangingProperties);
			}

			foreach(var cpe in vmChangingPropertiesExpressions) {
				string vmChangingPropertyName = GetPropertyName(cpe);
				if(!vmChangingProperties.Contains(vmChangingPropertyName)) {
					vmChangingProperties.Add(vmChangingPropertyName);
				}
			}
		}

		protected void OnEntityPropertyChanged(Action onChangeAction, params Expression<Func<TEntity, object>>[] entityTriggeredProperty)
		{
			foreach(var property in entityTriggeredProperty) {
				string entityPropertyName = Entity.GetPropertyName(property);
				if(!propertyOnChangeActions.ContainsKey(entityPropertyName)) {
					propertyOnChangeActions.Add(entityPropertyName, new List<Action>());
				}
				propertyOnChangeActions[entityPropertyName].Add(onChangeAction);
			}
		}

		protected void OnEntityAnyPropertyChanged(Action onChangeAction)
		{
			onAnyPropertyChangedActions.Add(onChangeAction);
		}

		#endregion
	}
}
