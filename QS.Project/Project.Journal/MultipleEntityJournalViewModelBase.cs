using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;

namespace QS.Project.Journal
{
	public abstract class MultipleEntityJournalViewModelBase<TNode> : EntitiesJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase
	{
		protected MultipleEntityJournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager = null) : base(unitOfWorkFactory, commonServices, navigationManager)
		{
		}
	}
}
