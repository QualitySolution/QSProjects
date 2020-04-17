using System;
using System.Runtime.CompilerServices;
using QS.DomainModel.UoW;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public class JournalFilterViewModelBase<TFilter> : ViewModelBase, IDisposable, IJournalFilterViewModel
		where TFilter : JournalFilterViewModelBase<TFilter>
	{
		public bool HidenByDefault { get; set; }

		private bool canNotify = true;

		private IUnitOfWork uow;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public virtual IUnitOfWork UoW {
			get {
				if(uow == null)
					uow = unitOfWorkFactory.CreateWithoutRoot();

				return uow;
			}
		}

		public JournalViewModelBase JournalViewModel { get; }

		public JournalFilterViewModelBase(JournalViewModelBase journalViewModel, IUnitOfWorkFactory unitOfWorkFactory = null)
		{
			JournalViewModel = journalViewModel ?? throw new ArgumentNullException(nameof(journalViewModel));
			this.unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Update()
		{
			if(canNotify)
				JournalViewModel.Refresh();
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			base.OnPropertyChanged(propertyName);
			Update();
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
