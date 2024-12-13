using Gamma.Binding.Converters;
using QS.BusinessCommon.Domain;
using QS.Project.Services;

namespace QSBusinessCommon
{
	public partial class MeasurementUnitsDlg : QS.Dialog.Gtk.EntityDialogBase<MeasurementUnits>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public MeasurementUnitsDlg ()
		{
			this.Build ();
			UoW = ServicesConfig.UnitOfWorkFactory.Create();
			Entity = new MeasurementUnits();
			ConfigureDlg ();
		}

		public MeasurementUnitsDlg (int id)
		{
			this.Build ();
			UoW = ServicesConfig.UnitOfWorkFactory.Create();
			Entity = UoW.GetById<MeasurementUnits>(id);
			ConfigureDlg ();
		}

		public MeasurementUnitsDlg (MeasurementUnits sub): this(sub.Id) {}


		private void ConfigureDlg ()
		{
			entryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();
			dataentryOKEI.Binding.AddBinding (Entity, e => e.OKEI, w => w.Text).InitializeFromSource ();
			spinDigits.Binding.AddBinding (Entity, e => e.Digits, w => w.ValueAsInt, new NumbersTypeConverter()).InitializeFromSource ();
		}

		public override bool Save ()
		{
			var validator = ServicesConfig.ValidationService;
			if (!validator.Validate(Entity))
				return false;

			logger.Info ("Сохраняем единицы измерения...");
			UoW.Save(Entity);
			UoW.Commit();
			return true;
		}

	}
}

