using System;

namespace QS.Project.Core {
	/// <summary>
	/// Используется для вызова устаревших статических регистраций в момент 
	/// инициализации подключения базы данных в контейнере зависимостей.
	/// </summary>
	[Obsolete("Хак для вызова инициализации устаревшей статики в контейнере")]
	public class OnDatabaseInitialization {
	}
}
