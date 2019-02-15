using FluentNHibernate.Mapping;
using QS.Project.Domain;

namespace QS.Project.HibernateMapping
{
	public class TypeOfEntityMap : ClassMap<TypeOfEntity>
	{
		public TypeOfEntityMap()
		{
			Table("types_of_entities");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CustomName).Column("entity_custom_name");
			Map(x => x.Type).Column("entity_type");
		}
	}
}
