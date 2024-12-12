using System;
using FluentNHibernate.Mapping;
using QS.Test.TestApp.Domain;

namespace QS.Test.TestApp.HibernateMapping
{
	public class Document1Map : ClassMap<Document1>
	{
		public Document1Map()
		{
			Id(x => x.Id);

			Map(x => x.Date);
		}
	}
}
