using System;
using System.Collections.Generic;
using QSOrmProject;
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
				.Where(n => n.Name.Contains("шт"))
				.Take(1).SingleOrDefault();
		}

		public static MeasurementUnits GetDefaultGoodsService(IUnitOfWork uow) {
			return uow.Session.QueryOver<MeasurementUnits>()
				.Where(n => n.Name.Contains("усл"))
				.Take(1).SingleOrDefault();
		}
	}
}

