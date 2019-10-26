using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain
{
	[Appellative(Nominative = "Документ 2")]
	public class Document2 : PropertyChangedBase, IDomainObject
	{
		public Document2()
		{
		}

		public Document2(DateTime date)
		{
			Date = date;
		}

		public virtual int Id { get; set; }

		public virtual DateTime Date { get; set; }
	}
}
