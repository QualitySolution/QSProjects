using QS.DomainModel.UoW;
using QS.Services;

namespace QS.Project.Journal
{
	public abstract class MultipleEntityJournalViewModelBase<TNode> : EntitiesJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase
	{
		protected MultipleEntityJournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(unitOfWorkFactory, commonServices)
		{
		}
	}
}
