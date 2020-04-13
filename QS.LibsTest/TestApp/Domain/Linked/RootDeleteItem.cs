using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain.Linked
{
	public class RootDeleteItem : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string Text { get; set; }
	}
}
