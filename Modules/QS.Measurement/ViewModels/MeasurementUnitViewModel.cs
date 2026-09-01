using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.Measurement.Domain;
using QS.DomainModel.NotifyChange;

namespace QS.Measurement.ViewModels
{
	public class MeasurementUnitViewModel : EntityDialogViewModelBase<MeasurementUnit>
	{
		public MeasurementUnitViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWork unitOfWork,
			INavigationManager navigation,
			IValidator validator,
			IEntityChangeWatcher changeWatcher = null
			) : base(uowBuilder, unitOfWork, navigation, validator, changeWatcher)
		{
		}
	}
}
