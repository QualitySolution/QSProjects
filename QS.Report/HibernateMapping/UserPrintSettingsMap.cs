using FluentNHibernate.Mapping;
using QS.Report.Domain;

namespace QS.Report.HibernateMapping
{
	public class UserPrintSettingsMap : ClassMap<UserPrintSettings>
	{
		public UserPrintSettingsMap()
		{
			Table("user_print_settings");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.PageOrientation).Column("page_orientation");
			Map(x => x.NumberOfCopies).Column("number_of_copies");
			References(x => x.User).Column("user_id");
		}
	}
}
