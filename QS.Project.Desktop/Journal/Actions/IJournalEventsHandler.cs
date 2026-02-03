using System.Collections.Generic;

namespace QS.Journal.Actions
{
	/// <summary>
	/// Интерфейс для обработки событий журнала.
	/// По сути внутренний интерфейс view model действий журнала, необходим для вызова событий со стороны view.
	/// </summary>
	public interface IJournalEventsHandler
	{
		/// <summary>
		/// Обработка двойного клика по ячейке
		/// </summary>
		/// <param name="node">Узел журнала</param>
		/// <param name="columnTag">Тег колонки</param>
		/// <param name="subcolumnTag">Тег подколонки</param>
		void OnCellDoubleClick(object node, object columnTag, object subcolumnTag);
		
		/// <summary>
		/// Обработка изменения выбора
		/// </summary>
		/// <param name="nodes">Выбранные узлы</param>
		void OnSelectionChanged(IList<object> nodes);
		
		/// <summary>
		/// Обработка нажатия клавиши
		/// </summary>
		/// <param name="key">Клавиша</param>
		void OnKeyPressed(string key);
	}
}
