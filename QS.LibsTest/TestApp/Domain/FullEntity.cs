using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain
{
	public class FullEntity: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public FullEntity()
		{
		}
	}
}
