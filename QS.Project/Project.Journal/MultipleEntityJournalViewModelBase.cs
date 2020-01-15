using QS.DomainModel.UoW;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;

namespace QS.Project.Journal
{
	public abstract class MultipleEntityJournalViewModelBase<TNode> : EntitiesJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase
	{
		protected MultipleEntityJournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, ICriterionSearch criterionSearch) : base(unitOfWorkFactory, commonServices, criterionSearch)
		{
		}
	}
}
