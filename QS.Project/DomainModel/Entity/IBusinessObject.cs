using System;
using QS.DomainModel.UoW;

namespace QS.DomainModel.Entity
{
	public interface IBusinessObject
	{
		IUnitOfWork UoW {set;}
	}
}

