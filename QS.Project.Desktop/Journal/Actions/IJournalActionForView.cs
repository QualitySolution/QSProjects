using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace QS.Journal.Actions
{
	/// <summary>
	/// Интерфейс для представления действия во View (не дженерик)
	/// </summary>
	public interface IJournalActionForView : INotifyPropertyChanged
	{
		/// <summary>
		/// Название действия
		/// </summary>
		string Title { get; }
		
		/// <summary>
		/// Доступность действия
		/// </summary>
		bool Sensitive { get; }
		
		/// <summary>
		/// Видимость действия
		/// </summary>
		bool Visible { get; }
		
		/// <summary>
		/// Дочерние действия (для выпадающих меню)
		/// </summary>
		IEnumerable<IJournalActionForView> ChildActionsView { get; }
		
		/// <summary>
		/// Выполнить действие
		/// </summary>
		void Execute();
	}
}
