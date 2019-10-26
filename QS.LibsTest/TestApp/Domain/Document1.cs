using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain
{
	[Appellative(Nominative = "Документ 1")]
	public class Document1 : PropertyChangedBase, IDomainObject
	{
		public Document1()
		{
		}

		public Document1(DateTime date)
		{
			Date = date;
		}

		public virtual int Id { get; set; }

		public virtual DateTime Date { get; set; }
	}
}
