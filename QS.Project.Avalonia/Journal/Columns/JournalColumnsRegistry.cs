using System;
using System.Collections.Generic;

namespace QS.Journal.Columns;

/// <summary>Здесь приложение описывает колонки своих журналов</summary>
public class JournalColumnsRegistry {
	private readonly Dictionary<Type, Func<IJournalViewModel, IJournalColumnsConfig>> configs =
		new Dictionary<Type, Func<IJournalViewModel, IJournalColumnsConfig>>();

	public JournalColumnsRegistry Register<TJournalViewModel>(Func<IJournalColumnsConfig> config)
		where TJournalViewModel : IJournalViewModel {
		configs.Add(typeof(TJournalViewModel), _ => config());
		return this;
	}

	/// <summary>Перегрузка на случай, когда состав колонок зависит от настроек самого журнала</summary>
	public JournalColumnsRegistry Register<TJournalViewModel>(Func<TJournalViewModel, IJournalColumnsConfig> config)
		where TJournalViewModel : IJournalViewModel {
		configs.Add(typeof(TJournalViewModel), journal => config((TJournalViewModel)journal));
		return this;
	}

	/// <summary>Колонки журнала или null, если для него колонки кодом не описаны</summary>
	// Описание строится в момент открытия журнала, а не регистрации, — поэтому его и хранит функция
	public IJournalColumnsConfig? Resolve(IJournalViewModel journal) =>
		configs.TryGetValue(journal.GetType(), out var config) ? config(journal) : null;
}
