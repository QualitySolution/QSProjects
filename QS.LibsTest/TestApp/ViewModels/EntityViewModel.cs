using System;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Test.TestApp.Domain;
using QS.ViewModels;

namespace QS.Test.TestApp.ViewModels
{
	public class EntityViewModel : EntityTabViewModelBase<FullEntity>
	{
		public EntityViewModel(IEntityUoWBuilder entityUoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(entityUoWBuilder, unitOfWorkFactory, commonServices)
		{
		}
	}
}
