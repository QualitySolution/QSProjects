using System;
using QS.DomainModel.Entity;

namespace QS.Test.TestApp.Domain
{
	public class SimpleEntity:  IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string Text { get; set; }

		public SimpleEntity()
		{
		}
	}
}
