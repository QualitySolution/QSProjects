using System.ComponentModel.DataAnnotations;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public abstract class EntityPermissionExtendedBase : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual PermissionExtendedType PermissionExtendedType { get;}

		private bool? isPermissionAvailable;
		[Display(Name = "Доступно ли право ?")]
		public virtual bool? IsPermissionAvailable {
			get => isPermissionAvailable;
			set => SetField(ref isPermissionAvailable, value);
		}

		private string permissionId;
		public virtual string PermissionId {
			get => permissionId;
			set => SetField(ref permissionId, value);
		}

		private TypeOfEntity typeOfEntity;
		[Display(Name = "Тип сущности")]
		public virtual TypeOfEntity TypeOfEntity {
			get => typeOfEntity;
			set => SetField(ref typeOfEntity, value, () => TypeOfEntity);
		}
	}

	public enum PermissionExtendedType
	{
		User,
		Subdivision
	}

	public class PermissionExtendedTypeStringType : NHibernate.Type.EnumStringType
	{
		public PermissionExtendedTypeStringType() : base(typeof(PermissionExtendedType))
		{
		}
	}
}
