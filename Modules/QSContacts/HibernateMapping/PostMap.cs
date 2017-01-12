using System;
using FluentNHibernate.Mapping;

namespace QSContacts.HMap
{
	public class PostMap : ClassMap<Post>
	{
		public PostMap ()
		{
			Table("post");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}

