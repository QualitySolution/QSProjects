using System;
using QS.DomainModel.Entity.EntityPermissions;

namespace QS.Permissions
{
	//FIXME Временная заглушка для валидатора прав, пока не появится возмоность его просто не настраивать, в тех случаях когда права не используются.
	public class EverythingAllowedEntityPermissionValidator : IEntityPermissionValidator
	{
		public EntityPermission Validate(Type entityType, int userId)
		{
			return new EntityPermission(true, true, true, true);
		}

		public EntityPermission Validate<TEntityType>(int userId)
		{
			return new EntityPermission(true, true, true, true);
		}
	}
}
