﻿using System;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Utilities.Text;
using QS.Validation;

namespace QS.ViewModels.Dialog
{
	public class EntityDialogViewModelBase<TEntity> : UowDialogViewModelBase
		where TEntity : class, IDomainObject, new()
	{
		protected readonly IEntityChangeWatcher ChangeWatcher;
		public override IUnitOfWork UoW => UoWGeneric;

		public IUnitOfWorkGeneric<TEntity> UoWGeneric { get; private set; }

		public TEntity Entity => UoWGeneric.Root;

		public EntityDialogViewModelBase(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null,
			IEntityChangeWatcher changeWatcher = null
			) : base(unitOfWorkFactory, navigation, validator, unitOfWorkProvider: unitOfWorkProvider)
		{
			if(uowBuilder == null) {
				throw new ArgumentNullException(nameof(uowBuilder));
			}

			this.ChangeWatcher = changeWatcher;

			string actionTitle = null;
			if(typeof(TEntity).GetSubjectNames()?.Genitive != null)
				actionTitle = $"Редактирование {typeof(TEntity).GetSubjectNames().Genitive}";

			UoWGeneric = uowBuilder.CreateUoW<TEntity>(unitOfWorkFactory, actionTitle);
			if (Entity == null) {
				AppellativeAttribute names = DomainHelper.GetSubjectNames(typeof(TEntity));
				throw new AbortCreatingPageException($"Загрузить [{names.Nominative}:{uowBuilder.EntityOpenId}] не удалось. " +
				                                     $"Возможно другой пользователь удалил этот объект.", "Ошибка открытия диалога");
			}
			if(unitOfWorkProvider != null)
				unitOfWorkProvider.UoW = UoW;
			base.Title = GetDialogNameByEntity();
			if(Entity is INotifyPropertyChanged propertyChanged)
				propertyChanged.PropertyChanged += Entity_PropertyChanged;

			Validations.Add(new ValidationRequest(Entity));
			
			ChangeWatcher?.BatchSubscribeOnEntity<TEntity>(ExternalDelete);
		}

		#region Создание имени диалога

		bool isAutoTitle = true;

		void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(isAutoTitle && (e.PropertyName == "Title" || e.PropertyName == "Name"))
				base.Title = GetDialogNameByEntity();
		}

		public override string Title {
			get => base.Title; 
			set {
				base.Title = value;
				isAutoTitle = false;
			}
		}

		private string GetDialogNameByEntity()
		{
			AppellativeAttribute names = DomainHelper.GetSubjectNames(Entity);

			if(UoW.IsNew) {
				if(names != null && !string.IsNullOrWhiteSpace(names.Nominative)) {
					switch(names.Gender) {
						case GrammaticalGender.Masculine:
							return "Новый " + names.Nominative;
						case GrammaticalGender.Feminine:
							return "Новая " + names.Nominative;
						case GrammaticalGender.Neuter:
							return "Новое " + names.Nominative;
						default:
							return "Новый(ая) " + names.Nominative;
					}
				}
			}
			else {
				var title = DomainHelper.GetTitle(Entity);

				if(String.IsNullOrWhiteSpace(title) && !String.IsNullOrWhiteSpace(names?.Nominative))
					title = names.Nominative.StringToTitleCase();

				return title;
			}
			return Entity.ToString();
		}
		#endregion

		private void ExternalDelete(EntityChangeEvent[] changeEvents) {
			if(changeEvents.Any(changeEvent => changeEvent.EventType == TypeOfChangeEvent.Delete && ((IDomainObject) changeEvent.Entity).Id == Entity.Id))
				NavigationManager.ForceClosePage(NavigationManager.FindPage(this));
		}

		public override void Dispose()
		{
			ChangeWatcher?.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}
