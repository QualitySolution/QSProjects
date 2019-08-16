using QS.BusinessCommon.Domain;
using QSBusinessCommon;
using QSOrmProject;
using QSOrmProject.DomainMapping;

namespace QS.BusinessCommon
{
	public static class MeasurementUnitsOrmMapping
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
