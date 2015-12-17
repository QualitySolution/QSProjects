using System;
using System.Collections.Generic;
using QSBusinessCommon;
using QSOrmProject;

namespace workwear.Repository
{
	public static class MeasurementUnitsRepository
	{
		public static IList<MeasurementUnits> GetActiveUnits(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<MeasurementUnits> ().List ();
		}
	}
}

