using Gamma.Binding.Converters;
using QS.Measurement.ViewModels;
using QS.Views.Dialog;
using QS.Measurement.Domain;

namespace QS.Measurement.Views
{
	public partial class MeasurementUnitView : EntityDialogViewBase<MeasurementUnitViewModel, MeasurementUnit>
	{
		public MeasurementUnitView(MeasurementUnitViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryOKEI.Binding.AddBinding(Entity, e => e.OKEI, w => w.Text).InitializeFromSource();
			spinDigits.Binding.AddBinding(Entity, e => e.Digits, w => w.ValueAsInt, new NumbersTypeConverter()).InitializeFromSource();
		}
	}
}
