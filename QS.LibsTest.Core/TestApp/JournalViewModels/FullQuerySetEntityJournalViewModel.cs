﻿using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Test.TestApp.Domain;
using QS.Test.TestApp.ViewModels;

namespace QS.Test.TestApp.JournalViewModels
{
	public class FullQuerySetEntityJournalViewModel : EntityJournalViewModelBase<Document1, EntityDialogViewModel, FullQuerySetDocumentJournalNode>
	{
		public FullQuerySetEntityJournalViewModel (IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigationManager, IEntityChangeWatcher changeWatcher) 
			: base(unitOfWorkFactory, navigationManager, changeWatcher)
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
