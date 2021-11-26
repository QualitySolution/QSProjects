using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;

namespace QS.Project.Journal
{
	public static class TreeViewColumnsConfigFactory
	{
		private static Dictionary<Type, Func<JournalViewModelBase, IColumnsConfig>> columnsConfigs = new Dictionary<Type, Func<JournalViewModelBase, IColumnsConfig>>();

		public static void Register<TJournalViewModel>(Func<IColumnsConfig> columnsConfigFunc)
			where TJournalViewModel : JournalViewModelBase
		{
			Type journalType = typeof(TJournalViewModel);
			if(columnsConfigs.ContainsKey(journalType))
				throw new InvalidOperationException($"Конфигурация колонок для модели представления \"{journalType.Name}\" уже зарегистрирована");
			columnsConfigs.Add(journalType, (jvm) => columnsConfigFunc());
		}

		public static void Register<TJournalViewModel>(Func<TJournalViewModel, IColumnsConfig> columnsConfigFunc)
			where TJournalViewModel : JournalViewModelBase
		{
			Type journalType = typeof(TJournalViewModel);
			if(columnsConfigs.ContainsKey(journalType))
				throw new InvalidOperationException($"Конфигурация колонок для модели представления \"{journalType.Name}\" уже зарегистрирована");
			columnsConfigs.Add(journalType, (jvm) => columnsConfigFunc((TJournalViewModel)jvm));
		}

		public static IColumnsConfig Resolve(JournalViewModelBase journalViewModel)
		{
			if(journalViewModel == null) {
				throw new ArgumentNullException(nameof(journalViewModel));
			}
			if(journalViewModel is SimpleEntityJournalViewModelBase) {
				return FluentColumnsConfig<CommonJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Название").AddTextRenderer(x => x.Title)
					.Finish();
			}
			var journalType = journalViewModel.GetType();

			if(!columnsConfigs.ContainsKey(journalType))
				throw new ApplicationException($"Не настроено сопоставление конфигурации колонок для модели представления \"{journalType.Name}\"");
			return columnsConfigs[journalType].Invoke(journalViewModel);
		}
	}
}