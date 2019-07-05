using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using QS.Project.Journal;

namespace QS.Journal.GtkUI
{
	public static class TreeViewColumnsConfigFactory
	{
		private static Dictionary<Type, Func<IColumnsConfig>> columnsConfigs = new Dictionary<Type, Func<IColumnsConfig>>();

		public static void Register<TJournalViewModel>(Func<IColumnsConfig> columnsConfigFunc)
			where TJournalViewModel : JournalViewModelBase
		{
			Type journalType = typeof(TJournalViewModel);
			if(columnsConfigs.ContainsKey(journalType))
				throw new InvalidOperationException($"Конфигурация колонок для модели представления \"{journalType.Name}\" уже зарегистрирована");
			columnsConfigs.Add(journalType, columnsConfigFunc);
		}

		public static IColumnsConfig Resolve<TJournalViewModel>()
		{
			Type journalType = typeof(TJournalViewModel);
			return Resolve(journalType);
		}

		public static IColumnsConfig Resolve(Type journalType)
		{
			if(journalType == null) {
				throw new ArgumentNullException(nameof(journalType));
			}
			if(typeof(SimpleEntityJournalViewModelBase).IsAssignableFrom(journalType)) {
				return FluentColumnsConfig<CommonJournalNode>.Create()
					.AddColumn("Номер").AddTextRenderer(x => x.Id.ToString())
					.AddColumn("Название").AddTextRenderer(x => x.Title)
					.Finish();
			}

			if(!columnsConfigs.ContainsKey(journalType))
				throw new ApplicationException($"Не настроено сопоставление конфигурации колонок для модели представления \"{journalType.Name}\"");
			return columnsConfigs[journalType].Invoke();
		}
	}
}