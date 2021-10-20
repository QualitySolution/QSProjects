using FluentNHibernate.Mapping;
using QS.Report.Domain;

namespace QS.Report.HibernateMapping
{
	public class UserSelectedPrinterMap : ClassMap<UserSelectedPrinter>
	{
		public UserSelectedPrinterMap()
		{
			Table("user_selected_printers");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			References(x => x.User).Column("user_id");
		}
	}
}
