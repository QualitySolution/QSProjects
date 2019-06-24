using System;
using QS.Services;
using QS.DomainModel.Config;
using System.Reflection;
namespace QS.Project.Journal.EntitySelector
{
	public class DefaultEntitySelectorFactory<TJournalViewModel, TJournalFilterViewModel> : IEntitySelectorFactory
		where TJournalViewModel : JournalViewModelBase, IEntitySelector
		where TJournalFilterViewModel : class, IJournalFilter
	{
		private readonly ICommonServices commonServices;
		private ConstructorInfo journalConstructorInfo;
		private ConstructorInfo filterConstructorInfo;
		private TJournalFilterViewModel filter;

		public DefaultEntitySelectorFactory(ICommonServices commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			Type filterType = typeof(TJournalFilterViewModel);
			filterConstructorInfo = filterType.GetConstructor(new Type[] { typeof(IInteractiveService) });
			if(filterConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для фильтра {filterType.Name} с параметром {nameof(IInteractiveService)}");
			}

			Type journalType = typeof(TJournalViewModel);
			journalConstructorInfo = journalType.GetConstructor(new Type[] { filterType, typeof(IEntityConfigurationProvider), typeof(ICommonServices) });
			if(journalConstructorInfo == null) {
				throw new ArgumentException($"Невозможно найти конструктор для журнала {journalType.Name} с параметрами:" +
					$"{filterType.Name}, {nameof(IEntityConfigurationProvider)}, {nameof(ICommonServices)}");
			}
		}

		public IEntitySelector CreateSelector()
		{
			IEntityConfigurationProvider entityConfigurationProvider = new DefaultEntityConfigurationProvider();

			filter = (TJournalFilterViewModel)filterConstructorInfo.Invoke(new object[] { commonServices.InteractiveService });
			var selectorViewModel = (TJournalViewModel)journalConstructorInfo.Invoke(new object[] { filter, entityConfigurationProvider, commonServices });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
