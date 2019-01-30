using FluentNHibernate.Mapping;
using QS.Project.Domain;

namespace QS.Project.HibernateMapping
{
	public class EntityUserPermissionMap :  ClassMap<EntityUserPermission>
	{
		public EntityUserPermissionMap()
		{
			Table("permission_entity_user");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CanCreate).Column("can_create");
			Map(x => x.CanRead).Column("can_read");
			Map(x => x.CanUpdate).Column("can_update");
			Map(x => x.CanDelete).Column("can_delete");

			References(x => x.User).Column("user_id");
			References(x => x.TypeOfEntity).Column("type_of_entity_id");
		}
	}
}
