using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain.Linked
{
	public class DependDeleteItem : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual AlsoDeleteItem CleanLink { get; set; }
		public virtual RootDeleteItem DeleteLink { get; set; }
	}
}
