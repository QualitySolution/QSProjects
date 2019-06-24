using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;

namespace QS.Journal.GtkUI
{
	public static class TreeViewColumnsConfigFactory
	{
		public static FluentColumnsConfig<TNode> Create<TNode>() => new FluentColumnsConfig<TNode>();

		static Dictionary<Type, IColumnsConfig> columnsConfigs = new Dictionary<Type, IColumnsConfig>();

		public static void Register<TJournalViewModel>(IColumnsConfig config)
		{
			Type journalType = typeof(TJournalViewModel);
			if(columnsConfigs.ContainsKey(journalType))
				throw new InvalidOperationException($"Конфигурация колонок для модели представления \"{journalType.Name}\" уже зарегистрирована");
			columnsConfigs.Add(journalType, config);
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

			if(!columnsConfigs.ContainsKey(journalType))
				throw new ApplicationException($"Не настроено сопоставление конфигурации колонок для модели представления \"{journalType.Name}\"");
			return columnsConfigs[journalType];
		}
	}
}