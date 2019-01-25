using FluentNHibernate.Mapping;
using QS.Project.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Persister.Entity;
using System;
using System.Collections;
using System.Linq.Expressions;

namespace QS.Project.Repositories
{
	public class EntityUserPermissionMap :  ClassMap<EntityUserPermission>
	{
		public EntityUserPermissionMap()
		{
			Table("permission_entity_user");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.EntityName).Column("entity_name");
			Map(x => x.CanCreate).Column("can_create");
			Map(x => x.CanRead).Column("can_read");
			Map(x => x.CanUpdate).Column("can_update");
			Map(x => x.CanDelete).Column("can_delete");

			References(x => x.User).Column("user_id");
		}
	}
}
