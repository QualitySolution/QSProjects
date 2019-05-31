using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestDomain
{
	public class BusinessObjectTestEntity : BusinessObjectBase<BusinessObjectTestEntity>, IDomainObject
	{
		public virtual int Id { get; set; }

		public BusinessObjectTestEntity()
		{
		}
	}
}
