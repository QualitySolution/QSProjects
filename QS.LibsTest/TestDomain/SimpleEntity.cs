using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestDomain
{
	public class SimpleEntity: IDomainObject
	{
		public virtual int Id { get; set; }

		public SimpleEntity()
		{
		}
	}
}
