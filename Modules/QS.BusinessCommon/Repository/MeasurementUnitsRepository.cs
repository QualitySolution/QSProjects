using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.BusinessCommon.Domain;

namespace QS.BusinessCommon.Repository
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


		public static Func<IUnitOfWork, string, MeasurementUnits> GetUnitsByOKEITestGap;
		public static MeasurementUnits GetUnitsByOKEI(IUnitOfWork uow, string okei)
		{
			if(GetUnitsByOKEITestGap != null)
				return GetUnitsByOKEITestGap(uow, okei);

			return uow.Session.QueryOver<MeasurementUnits>()
				.Where(n => n.OKEI == okei)
				.Take(1).SingleOrDefault();
		}
	}
}

