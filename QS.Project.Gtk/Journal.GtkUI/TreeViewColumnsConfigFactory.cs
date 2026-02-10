using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using QS.Project.Journal;

namespace QS.Journal.GtkUI
{
	public static class TreeViewColumnsConfigFactory
	{
		private static Dictionary<Type, Func<IJournalViewMode, IColumnsConfig>> columnsConfigs = new Dictionary<Type, Func<IJournalViewMode, IColumnsConfig>>();

		public static void Register<TJournalViewModel>(Func<IColumnsConfig> columnsConfigFunc)
			where TJournalViewModel : IJournalViewMode
		{
			Type journalType = typeof(TJournalViewModel);
			if(columnsConfigs.ContainsKey(journalType))
				throw new InvalidOperationException($"Конфигурация колонок для модели представления \"{journalType.Name}\" уже зарегистрирована");
			columnsConfigs.Add(journalType, (jvm) => columnsConfigFunc());
		}

		public static void Register<TJournalViewModel>(Func<TJournalViewModel, IColumnsConfig> columnsConfigFunc)
			where TJournalViewModel : IJournalViewMode
		{
			Type journalType = typeof(TJournalViewModel);
			if(columnsConfigs.ContainsKey(journalType))
				throw new InvalidOperationException($"Конфигурация колонок для модели представления \"{journalType.Name}\" уже зарегистрирована");
			columnsConfigs.Add(journalType, (jvm) => columnsConfigFunc((TJournalViewModel)jvm));
		}

		public static IColumnsConfig Resolve(IJournalViewMode journalViewModel)
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
