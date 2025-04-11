using System;
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
		protected ICurrentPermissionService PermissionService { get; }
		protected IPermissionResult EntityPermission { get; private set; }

		public PermittingEntityDialogViewModelBase(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ICurrentPermissionService permissionService,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			PermissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
			EntityPermission = PermissionService.ValidateEntityPermission(typeof(TEntity));
			if(!EntityPermission.CanRead) {
				var names = DomainHelper.GetSubjectNames(typeof(TEntity));
				throw new AbortCreatingPageException($"У вас недостаточно прав для просмотра {names.Genitive ?? names.Nominative}", "Запрет доступа");
			}
		}

		protected Func<DateTime?> GetDocumentDateFunc {
			set => EntityPermission = PermissionService.ValidateEntityPermission(typeof(TEntity), value.Invoke());
		}
		
		public bool CanEdit => EntityPermission.CanUpdate;
		
		protected bool CanDocumentDateChange(DateTime toDate) => PermissionService.ValidateEntityPermission(typeof(TEntity), toDate).CanUpdate;
	}
}
