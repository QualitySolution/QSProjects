using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;

namespace QS.Test.TestApp.JournalViewModels
{
	public class DocumentsJournalViewModel : MultipleEntityJournalViewModelBase<DocumentJournalNode>
	{
		public DocumentsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(unitOfWorkFactory, commonServices)
		{
		}
	}

	public class DocumentJournalNode<TDocument> : DocumentJournalNode
		where TDocument : class, IDomainObject
	{
		protected DocumentJournalNode() : base(typeof(TDocument))
		{
		}
	}

	public class DocumentJournalNode : JournalEntityNodeBase
	{
		protected DocumentJournalNode(Type entityType) : base(entityType)
		{
		}

		public override string Title { get; }
		public DateTime Date { get; set; }
	}
}
