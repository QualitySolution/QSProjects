using System;
using QS.BusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.DomainMapping;

namespace QSBusinessCommon
{
	public static class OrmMapping
	{
		public static IOrmObjectMapping GetOrmMapping()
		{
			return OrmObjectMapping<MeasurementUnits>.Create().Dialog<MeasurementUnitsDlg>().DefaultTableView()
				.Column("ОКЕИ", i => i.OKEI)
				.SearchColumn("Наименование", i => i.Name)
				.Column("Точность", i => i.Digits.ToString()).End();
		}
	}
}
