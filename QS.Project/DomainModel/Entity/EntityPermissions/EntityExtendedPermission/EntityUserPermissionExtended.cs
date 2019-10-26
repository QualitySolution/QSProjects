using System.ComponentModel.DataAnnotations;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public class EntityUserPermissionExtended : EntityPermissionExtendedBase
	{
		public override PermissionExtendedType PermissionExtendedType => PermissionExtendedType.User; 

		private UserBase user;
		[Display(Name = "Пользователь")]
		public virtual UserBase User {
			get => user;
			set => SetField(ref user, value);
		}
	}
}
