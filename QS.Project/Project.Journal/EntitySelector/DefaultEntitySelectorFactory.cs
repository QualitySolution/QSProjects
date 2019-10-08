using System;
using QS.Services;
using QS.DomainModel.Config;
using System.Reflection;
using QS.DomainModel.Entity;
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
			filterConstructorInfo = filterType.GetConstructor(new Type[] { typeof(IInteractiveService) });
			if(filterConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для фильтра {filterType.Name} с параметром {nameof(IInteractiveService)}");
			}

			Type journalType = typeof(TJournalViewModel);
			journalConstructorInfo = journalType.GetConstructor(new Type[] { filterType, typeof(ICommonServices) });
			if(journalConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для журнала {journalType.Name} с параметрами:" +
					$"{filterType.Name}, {nameof(ICommonServices)}");
			}
		}

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			var filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { commonServices.InteractiveService });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, commonServices });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
