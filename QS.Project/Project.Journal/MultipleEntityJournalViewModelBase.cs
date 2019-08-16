using System;
using QS.DomainModel.Config;
using QS.Services;

namespace QS.Project.Journal
{
	public abstract class MultipleEntityJournalViewModelBase<TNode> : EntityJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase
	{
		protected MultipleEntityJournalViewModelBase(IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(entityConfigurationProvider, commonServices)
		{
		}
	}
}
