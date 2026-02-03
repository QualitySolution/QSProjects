using System;
using System.Collections.Generic;

namespace QS.Project.Journal.Actions
{
	/// <summary>
	/// По сути внутренний интрефейс вью модели действий журнала необходим для вызова событий со стороны вью.
	/// </summary>
	public interface IJournalEventsHandler
	{
		void OnCellDoubleClick(object node, object columnTag, object subcolumnTag);
		void OnSelectionChanged(IList<object> nodes);
		void OnKeyPressed(string key);
	}
}
