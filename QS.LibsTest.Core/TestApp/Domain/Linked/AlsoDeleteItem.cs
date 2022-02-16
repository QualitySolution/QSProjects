using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain.Linked
{
	public class AlsoDeleteItem : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual RootDeleteItem Root { get; set; }
	}
}
