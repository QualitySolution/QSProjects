using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.Test.TestApp.Domain;
using QS.Test.TestApp.ViewModels;

namespace QS.Test.TestApp.JournalViewModels
{
	public class FullQuerySetJustEntityJournalViewModel : JustEntityJournalViewModelBase<Document1, EntityViewModel, JustDocumentJournalNode>
	{
		public FullQuerySetJustEntityJournalViewModel (IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService) : base(unitOfWorkFactory, interactiveService)
		{

		}

		protected override IQueryOver<Document1> ItemsQuery(IUnitOfWork uow)
		{
			JustDocumentJournalNode resultAlias = null;
			return uow.Session.QueryOver<Document1>()
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Date).WithAlias(() => resultAlias.Date)
				).TransformUsing(Transformers.AliasToBean<JustDocumentJournalNode>());
		}
	}

	public class JustDocumentJournalNode
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
	}
}