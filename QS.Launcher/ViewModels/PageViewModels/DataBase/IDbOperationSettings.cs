using System;
using System.Collections.Generic;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	/// <summary>
	/// Одна операция мастера настроек базы
	/// </summary>
	public interface IDbOperationSettings {
		/// <summary>Заголовок страницы</summary>
		string Title { get; }

		/// <summary>валидность ввода операции</summary>
		IObservable<bool> CanProceed { get; }

		/// <summary>состав фаз операции</summary>
		IEnumerable<DbCreationPhase> BuildPipeline();
	}
}
