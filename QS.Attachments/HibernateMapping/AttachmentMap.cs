using FluentNHibernate.Mapping;
using QS.Attachments.Domain;

namespace QS.Attachments.HibernateMapping
{
	public class AttachmentMap : ClassMap<Attachment>
	{
		public AttachmentMap()
		{
			Table("files");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FileName).Column("file_name");
			Map(x => x.ByteFile).Column("file").CustomSqlType("BinaryBlob").LazyLoad();
		}
	}
}
