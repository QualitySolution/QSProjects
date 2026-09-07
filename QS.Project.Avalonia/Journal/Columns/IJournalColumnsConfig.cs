using Avalonia.Controls;

namespace QS.Journal.Columns;

public interface IJournalColumnsConfig {
	/// <summary>Собирает таблицу журнала с описанными колонками</summary>
	DataGrid MakeTable();
}
