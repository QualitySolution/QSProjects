using QS.DomainModel.UoW;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;

namespace QS.Project.Journal
{
	public abstract class MultipleEntityJournalViewModelBase<TNode, TSearchModel> : EntitiesJournalViewModelBase<TNode, TSearchModel>
		where TNode : JournalEntityNodeBase
		where TSearchModel : CriterionSearchModelBase
	{
		protected MultipleEntityJournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, SearchViewModelBase<TSearchModel> searchViewModel) : base(unitOfWorkFactory, commonServices, searchViewModel)
		{
		}
	}
}
