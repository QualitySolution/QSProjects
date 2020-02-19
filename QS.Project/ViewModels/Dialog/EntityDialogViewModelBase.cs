using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
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
		public override IUnitOfWork UoW => UoWGeneric;

		public IUnitOfWorkGeneric<TEntity> UoWGeneric { get; private set; }

		public TEntity Entity => UoWGeneric.Root;

		public EntityDialogViewModelBase(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null) : base(unitOfWorkFactory, navigation)
		{
			if(uowBuilder == null) {
				throw new ArgumentNullException(nameof(uowBuilder));
			}
			UoWGeneric = uowBuilder.CreateUoW<TEntity>(unitOfWorkFactory);
			base.Title = GetDialogNameByEntity();
			if(Entity is INotifyPropertyChanged propertyChanged)
				propertyChanged.PropertyChanged += Entity_PropertyChanged;

			this.validator = validator;
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
				var title = DomainHelper.GetObjectTilte(Entity);

				if(String.IsNullOrWhiteSpace(title) && !String.IsNullOrWhiteSpace(names?.Nominative))
					title = names.Nominative.StringToTitleCase();

				return title;
			}
			return Entity.ToString();
		}

		#endregion

		#region Валидация сущности

		protected ValidationContext ValidationContext { get; set; }

		private readonly IValidator validator;

		protected virtual bool Validate()
		{
			return validator?.Validate(Entity, ValidationContext) ?? true;
		}

		public override bool Save()
		{
			if (!Validate())
				return false;
			return base.Save();
		}

		#endregion
	}
}
