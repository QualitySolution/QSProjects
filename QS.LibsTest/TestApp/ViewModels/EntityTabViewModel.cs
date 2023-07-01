using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Test.TestApp.Domain;
using QS.ViewModels;

namespace QS.Test.TestApp.ViewModels
{
	public class EntityTabViewModel : EntityTabViewModelBase<FullEntity>
	{
		public EntityTabViewModel(IEntityUoWBuilder entityUoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(entityUoWBuilder, unitOfWorkFactory, commonServices)
		{
		}
	}
}
