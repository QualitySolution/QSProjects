using System;
using FluentNHibernate.Mapping;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;

namespace QS.Project.HibernateMapping
{
	public class EntityPermissionExtendedBaseMap : ClassMap<EntityPermissionExtendedBase>
	{
		public EntityPermissionExtendedBaseMap()
		{
			Table("entity_permission_extended");

			DiscriminateSubClassesOnColumn("permission_type");
			Map(x => x.PermissionExtendedType).Column("permission_type").CustomType<PermissionExtendedTypeStringType>().Update().Not.Insert();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.PermissionId).Column("permission_id");


			Map(x => x.IsPermissionAvailable).Column("is_permission_available");

			References(x => x.TypeOfEntity).Column("type_of_entity_id");
		}
	}

	public class EntityUserExtendedPermissionExtendedMao : SubclassMap<EntityUserPermissionExtended>
	{
		public EntityUserExtendedPermissionExtendedMao()
		{
			DiscriminatorValue(PermissionExtendedType.User.ToString());
			References(x => x.User).Column("user_id");
		}
	}
}
