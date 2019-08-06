using System;
using QS.Services;
using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Utilities;

namespace QS.ViewModels
{
	public abstract class EntityWidgetViewModelBase<TEntity> : UoWWidgetViewModelBase
		where TEntity : class, IDomainObject
	{
		protected ICommonServices CommonServices { get; }

		protected IPermissionResult PermissionResult { get; private set; }

		public TEntity Entity { get; private set; }

		protected EntityWidgetViewModelBase(TEntity entity, ICommonServices commonServices) 
		: base((commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService)
		{
			Entity = entity;
			this.CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			PermissionResult = commonServices.PermissionService.ValidateUserPermission(typeof(TEntity), commonServices.UserService.CurrentUserId);
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
		protected void SetPropertyChangeRelation<T>(Expression<Func<TEntity, object>> entityTriggeredProperty, params Expression<Func<T>>[] vmChangingPropertiesExpressions)
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
