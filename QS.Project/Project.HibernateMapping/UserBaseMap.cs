using System;
using FluentNHibernate.Mapping;
using QS.Project.Domain;

namespace QS.Project.HibernateMapping
{
	public class UserBaseMap : ClassMap<UserBase>
	{
		public UserBaseMap()
		{
			Table("users");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Login).Column("login");
		}
	}
}
