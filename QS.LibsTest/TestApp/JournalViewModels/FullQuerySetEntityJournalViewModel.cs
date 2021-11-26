using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Test.TestApp.Domain;

namespace QS.Test.TestApp.JournalViewModels
{
	public class FullQuerySetEntityJournalViewModel : EntityJournalViewModelBase<Document1, FullQuerySetDocumentJournalNode>
	{
		public FullQuerySetEntityJournalViewModel (IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager) 
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{

		}

		protected override IQueryOver<Document1> ItemsQuery(IUnitOfWork uow)
		{
			FullQuerySetDocumentJournalNode resultAlias = null;
			return uow.Session.QueryOver<Document1>()
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Date).WithAlias(() => resultAlias.Date)
				).TransformUsing(Transformers.AliasToBean<FullQuerySetDocumentJournalNode>());
		}
	}

	public class FullQuerySetDocumentJournalNode
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
	}
}