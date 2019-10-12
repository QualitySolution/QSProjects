using System;
using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain;

namespace QS.Test.TestApp.HibernateMapping
{
	public class Document2Map : ClassMap<Document2>
	{
		public Document2Map()
		{
			Id(x => x.Id);

			Map(x => x.Date);
		}
	}
}
