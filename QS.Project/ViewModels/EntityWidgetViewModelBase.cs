using System;
using QS.Services;
using QS.DomainModel.Entity;

namespace QS.ViewModels
{
	public abstract class EntityWidgetViewModelBase<TEntity> : UoWWidgetViewModelBase
		where TEntity : class, IDomainObject
	{
		private readonly ICommonServices commonServices;

		protected IPermissionResult PermissionResult { get; private set; }

		public TEntity Entity { get; private set; }

		protected EntityWidgetViewModelBase(TEntity entity, ICommonServices commonServices) 
		: base((commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService)
		{
			Entity = entity;
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			PermissionResult = commonServices.PermissionService.ValidateUserPermission(typeof(TEntity), commonServices.UserService.CurrentUserId);
		}
	}
}
