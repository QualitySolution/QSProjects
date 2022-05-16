using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain.Entity
{
	public class BusinessObjectTestEntity : BusinessObjectBase<BusinessObjectTestEntity>, IDomainObject
	{
		public virtual int Id { get; set; }

		public BusinessObjectTestEntity()
		{
		}
	}
}
