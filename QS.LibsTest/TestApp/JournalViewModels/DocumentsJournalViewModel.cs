using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;

namespace QS.Test.TestApp.JournalViewModels
{
	public class DocumentsJournalViewModel : MultipleEntityJournalViewModelBase<DocumentJournalNode>
	{
		public DocumentsJournalViewModel(
			EntitiesJournalActionsViewModel journalActionsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(journalActionsViewModel, unitOfWorkFactory, commonServices)
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

		public DateTime Date { get; set; }
	}
}
