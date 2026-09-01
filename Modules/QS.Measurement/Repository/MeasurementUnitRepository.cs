using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Measurement.Domain;

namespace QS.Measurement.Repository
{
	public static class MeasurementUnitRepository
	{
		public static IList<MeasurementUnit> GetActiveUnits(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<MeasurementUnit> ().List ();
		}

		public static MeasurementUnit GetDefaultGoodsUnit(IUnitOfWork uow) {
			return uow.Session.QueryOver<MeasurementUnit>()
				.Where(n => n.Name.IsLike("шт%"))
				.Take(1).SingleOrDefault();
		}

		public static MeasurementUnit GetDefaultGoodsService(IUnitOfWork uow) {
			return uow.Session.QueryOver<MeasurementUnit>()
				.Where(n => n.Name.IsLike("усл%"))
				.Take(1).SingleOrDefault();
		}


		public static Func<IUnitOfWork, string, MeasurementUnit> GetUnitsByOKEITestGap;
		public static MeasurementUnit GetUnitsByOKEI(IUnitOfWork uow, string okei)
		{
			if(GetUnitsByOKEITestGap != null)
				return GetUnitsByOKEITestGap(uow, okei);

			return uow.Session.QueryOver<MeasurementUnit>()
				.Where(n => n.OKEI == okei)
				.Take(1).SingleOrDefault();
		}
	}
}
