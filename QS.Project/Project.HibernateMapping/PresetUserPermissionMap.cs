using System;
using FluentNHibernate.Mapping;
using QS.Project.Domain;

namespace QS.Project.HibernateMapping
{
	public class PresetUserPermissionMap : ClassMap<PresetUserPermission>
	{
		public PresetUserPermissionMap()
		{
			Table("permission_preset_user");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PermissionName).Column("permission_name");
			References(x => x.User).Column("user_id");
		}
	}
}
