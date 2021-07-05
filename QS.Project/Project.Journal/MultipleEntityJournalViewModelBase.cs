using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public abstract class MultipleEntityJournalViewModelBase<TNode> : EntitiesJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase
	{
		protected MultipleEntityJournalViewModelBase(
			EntitiesJournalActionsViewModel journalActionsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(journalActionsViewModel, unitOfWorkFactory, commonServices)
		{
		}
	}
}
