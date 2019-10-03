using System;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Test.TestApp.Domain;
using QS.ViewModels;

namespace QS.Test.TestApp.ViewModels
{
	public class EntityViewModel : EntityTabViewModelBase<FullEntity>
	{
		public EntityViewModel(IUnitOfWorkGeneric<FullEntity> uow, ICommonServices commonServices) : base(uow, commonServices)
		{
		}

		public EntityViewModel() : base(Substitute.For<IUnitOfWorkGeneric<FullEntity>>(), Substitute.For<ICommonServices>()) { }
	}
}
