using System;
using QS.DomainModel.UoW;

namespace QS.Models
{
	public abstract class UoWFactoryModelBase : ModelBase
	{
		public UoWFactoryModelBase(IUnitOfWorkFactory unitOfWorkFactory)
		{
			UnitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;
	}
}
