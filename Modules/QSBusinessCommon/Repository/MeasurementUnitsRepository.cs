using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSBusinessCommon.Domain;

namespace QSBusinessCommon.Repository
{
	public static class MeasurementUnitsRepository
	{
		public static IList<MeasurementUnits> GetActiveUnits(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<MeasurementUnits> ().List ();
		}

		public static MeasurementUnits GetDefaultGoodsUnit(IUnitOfWork uow) {
			return uow.Session.QueryOver<MeasurementUnits>()
				.Where(n => n.Name.IsLike("шт%"))
				.Take(1).SingleOrDefault();
		}

		public static MeasurementUnits GetDefaultGoodsService(IUnitOfWork uow) {
			return uow.Session.QueryOver<MeasurementUnits>()
				.Where(n => n.Name.IsLike("усл%"))
				.Take(1).SingleOrDefault();
		}
	}
}

