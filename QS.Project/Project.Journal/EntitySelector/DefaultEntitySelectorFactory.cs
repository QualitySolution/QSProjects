using System;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;

namespace QS.Project.Journal.EntitySelector
{
	public class DefaultEntitySelectorFactory<TEntity, TJournalViewModel, TJournalFilterViewModel> : IEntitySelectorFactory
		where TJournalViewModel : JournalViewModelBase, IEntitySelector
		where TJournalFilterViewModel : class, IJournalFilter
		where TEntity : class, IDomainObject
	{
		private readonly ICommonServices commonServices;
		protected ConstructorInfo journalConstructorInfo;
		protected ConstructorInfo filterConstructorInfo;

		public Type EntityType => typeof(TEntity);

		public DefaultEntitySelectorFactory(ICommonServices commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			Type filterType = typeof(TJournalFilterViewModel);
			filterConstructorInfo = filterType.GetConstructor(new Type[] {});
			if(filterConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для фильтра {filterType.Name}");
			}

			Type journalType = typeof(TJournalViewModel);
			journalConstructorInfo = journalType.GetConstructor(new Type[] { filterType, typeof(IUnitOfWorkFactory), typeof(ICommonServices) });
			if(journalConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для журнала {journalType.Name} с параметрами:" +
					$"{filterType.Name}, {nameof(IUnitOfWorkFactory)},  {nameof(ICommonServices)}");
			}
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, UnitOfWorkFactory.GetDefaultFactory, commonServices });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
