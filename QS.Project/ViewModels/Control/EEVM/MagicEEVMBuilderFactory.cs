using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.Navigation;

namespace QS.ViewModels.Control.EEVM
{

	/// <summary>
	/// Пример класса с более простой фабрикой которая может сама взять данные из EntityTabViewModelBase
	/// </summary>
	public class MagicEEVMBuilderFactory<TBindedEntity> : CommonEEVMBuilderFactory<TBindedEntity>
		where TBindedEntity: class, INotifyPropertyChanged, IDomainObject, new()
	{

		public MagicEEVMBuilderFactory(EntityTabViewModelBase<TBindedEntity> entityViewModel, INavigationManager navigationManager) :
			base(entityViewModel, entityViewModel.Entity, entityViewModel.UoW, navigationManager)
		{

		}
	}
}
