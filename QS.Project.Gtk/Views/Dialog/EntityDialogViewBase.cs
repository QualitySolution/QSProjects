using QS.DomainModel.Entity;
using QS.ViewModels.Dialog;

namespace QS.Views.Dialog
{
	public abstract class EntityDialogViewBase<TViewModel, TEntity> : SavedDialogViewBase<TViewModel>
		where TViewModel : EntityDialogViewModelBase<TEntity>
		where TEntity : class, IDomainObject, new()
	{
		protected TEntity Entity => ViewModel.Entity;

		protected EntityDialogViewBase(TViewModel viewModel): base(viewModel)
		{

		}
	}
}
