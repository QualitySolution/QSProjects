using System;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Validation;

namespace QS.ViewModels.Dialog {
	public class PermittingEntityDialogViewModelBase<TEntity> : EntityDialogViewModelBase<TEntity>
		where TEntity : class, IDomainObject, new()
	{
		private readonly IInteractiveMessage interactive;
		protected ICurrentPermissionService PermissionService { get; }
		protected IPermissionResult EntityPermission => PermissionService.ValidateEntityPermission(typeof(TEntity), DocumentDate);

		public PermittingEntityDialogViewModelBase(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ICurrentPermissionService permissionService,
			IInteractiveMessage interactive = null,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.interactive = interactive;
			PermissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
			if(!EntityPermission.CanRead) {
				var names = DomainHelper.GetSubjectNames(typeof(TEntity));
				throw new AbortCreatingPageException($"У вас недостаточно прав для просмотра {names.Genitive ?? names.Nominative}", "Запрет доступа");
			}
		}
		
		#region Документы с датой
		/// <summary>
		/// Устанавливает свойство даты документа. Дата документа используется для проверки прав доступа к документу.
		/// </summary>
		protected void SetDocumentDateProperty(Expression<Func<TEntity, DateTime>> propertyExpression) {
			datePropertyInfo = PropertyUtil.GetPropertyInfo(propertyExpression);
		}
		private PropertyInfo datePropertyInfo;
		
		/// <summary>
		/// Проброс свойства даты документа для View. Реализукт проверку прав на смедую даты документа в закрытом периоде.
		/// </summary>
		public virtual DateTime? DocumentDate {
			get => datePropertyInfo?.GetValue(Entity) as DateTime?;
			set {
				if(DocumentDate == value || value == null)
					return;
				if(CanDocumentDateChange(value.Value))
					datePropertyInfo.SetValue(Entity, value);
				else {
					OnPropertyChanged(); //Чтобы вернуть назад дату в виджете.
					interactive?.ShowMessage(ImportanceLevel.Error, "Нельзя изменить дату документа на закрытый период.");
				}
			}
		}
		#endregion
		
		public bool CanEdit => EntityPermission.CanUpdate;
		
		protected bool CanDocumentDateChange(DateTime toDate) => PermissionService.ValidateEntityPermission(typeof(TEntity), toDate).CanUpdate;
	}
}
