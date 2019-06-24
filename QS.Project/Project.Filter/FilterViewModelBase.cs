using System;
using System.Linq.Expressions;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Gamma.Utilities;
using System.Linq;
using QS.Dialog;

namespace QS.Project.Filter
{
	public abstract class FilterViewModelBase<TFilter> : WidgetViewModelBase, IDisposable, IJournalFilter, ISingleUoWDialog
		where TFilter : FilterViewModelBase<TFilter>
	{
		public event EventHandler OnFiltered;

		private bool canNotify = true;

		public virtual IUnitOfWork UoW { get; protected set; }

		protected FilterViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			PropertyChanged += FilterViewModelBase_PropertyChanged;
		}

		public void Update()
		{
			if(canNotify)
				OnFiltered?.Invoke(this, new EventArgs());
		}

		string[] updatedFilterProperties;

		protected void UpdateWith(params Expression<Func<TFilter, object>>[] filterProperties)
		{
			updatedFilterProperties = filterProperties.Select(PropertyUtil.GetName).ToArray();
		}

		void FilterViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(updatedFilterProperties != null && updatedFilterProperties.Contains(e.PropertyName)) {
				Update();
			}
		}



		/// <summary>
		/// Для установки свойств фильтра без перезапуска фильтрации на каждом изменении
		/// обновления журналов при каждом выставлении ограничения.
		/// </summary>
		/// <param name="setters">Лямбды ограничений</param>
		public void SetAndRefilterAtOnce(params Action<TFilter>[] setters)
		{
			canNotify = false;
			TFilter filter = this as TFilter;
			foreach(var item in setters) {
				item(filter);
			}
			canNotify = true;
			Update();
		}

		public void Dispose() => UoW?.Dispose();
	}
}