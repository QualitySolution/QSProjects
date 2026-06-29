using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Journal;
using QS.Measurement.ViewModels;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Services;
using QS.Measurement.Domain;

namespace QS.Measurement.Journal.ViewModels
{
	public class MeasurementUnitJournalViewModel : EntityJournalViewModelBase<MeasurementUnit, MeasurementUnitViewModel, MeasurementUnitJournalNode>
	{
		public MeasurementUnitJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			IEntityChangeWatcher changeWatcher,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, navigationManager, changeWatcher, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<MeasurementUnit> ItemsQuery(IUnitOfWork uow)
		{
			MeasurementUnitJournalNode resultAlias = null;

			return uow.Session.QueryOver<MeasurementUnit>()
				.Where(GetSearchCriterion<MeasurementUnit>(
					x => x.Id,
					x => x.Name,
					x => x.OKEI
				))
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.OKEI).WithAlias(() => resultAlias.OKEI)
					.Select(x => x.Digits).WithAlias(() => resultAlias.Digits)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<MeasurementUnitJournalNode>());
		}
	}

	public class MeasurementUnitJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string OKEI { get; set; }
		public short Digits { get; set; }
	}
}
