using Gamma.Binding.Converters;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Validation;

namespace QSBusinessCommon
{
	public partial class MeasurementUnitsDlg : QS.Dialog.Gtk.EntityDialogBase<MeasurementUnits>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public MeasurementUnitsDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<MeasurementUnits>();
			ConfigureDlg ();
		}

		public MeasurementUnitsDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<MeasurementUnits> (id);
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
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if (!validator.Validate(Entity))
				return false;

			logger.Info ("Сохраняем единицы измерения...");
			UoWGeneric.Save();
			return true;
		}

	}
}

