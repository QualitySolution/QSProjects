using System;
using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.ViewModels;

namespace QS.Views.GtkUI
{
	public class EntityTabViewBase<TViewModel, TEntity> : TabViewBase<TViewModel>
		where TViewModel : EntityTabViewModelBase<TEntity>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
	{
		protected TEntity Entity => ViewModel.Entity;

		public EntityTabViewBase(TViewModel viewModel): base(viewModel)
		{

		}
	}
}
