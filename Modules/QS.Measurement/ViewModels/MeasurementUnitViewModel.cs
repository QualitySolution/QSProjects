using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.Measurement.Domain;

namespace QS.Measurement.ViewModels
{
	public class MeasurementUnitViewModel : EntityDialogViewModelBase<MeasurementUnit>
	{
		public MeasurementUnitViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}
	}
}
