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
	}
}

