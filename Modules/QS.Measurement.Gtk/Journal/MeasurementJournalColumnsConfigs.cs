using Gamma.ColumnConfig;
using QS.Journal.GtkUI;
using QS.Measurement.Journal.ViewModels;

namespace QS.Measurement.Journal
{
	public static class MeasurementJournalColumnsConfigs
	{
		public static void RegisterColumns()
		{
			TreeViewColumnsConfigFactory.Register<MeasurementUnitJournalViewModel>(
				() => FluentColumnsConfig<MeasurementUnitJournalNode>.Create()
					.AddColumn("ИД").AddReadOnlyTextRenderer(node => node.Id.ToString()).XAlign(0.5f).SearchHighlight()
					.AddColumn("Название").AddReadOnlyTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("ОКЕИ").AddReadOnlyTextRenderer(node => node.OKEI).XAlign(0.5f).SearchHighlight()
					.AddColumn("Точность").AddReadOnlyTextRenderer(node => node.Digits.ToString()).XAlign(0.5f)
					.Finish()
			);
		}
	}
}
