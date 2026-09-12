using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Data;
using Gamma.Utilities;

namespace QS.Journal.Columns;

/// <summary>
/// Колонки журнала для узла <typeparamref name="TNode"/>
/// </summary>
public class ColumnsConfig<TNode> : IJournalColumnsConfig {
	private readonly List<DataGridColumn> columns = new List<DataGridColumn>();
	private DataGridTextColumn? current;

	private ColumnsConfig() {
	}

	public static ColumnsConfig<TNode> Create() => new ColumnsConfig<TNode>();

	/// <summary>Колонка из последнего AddColumn</summary>
	private DataGridTextColumn Current =>
		current ?? throw new InvalidOperationException("Сначала AddColumn, потом настройка колонки.");

	public ColumnsConfig<TNode> AddColumn(string title) {
		current = new DataGridTextColumn { Header = title };
		columns.Add(current);
		return this;
	}

	/// <param name="format">Формат значения, как у string.Format</param>
	public ColumnsConfig<TNode> AddTextRenderer(Expression<Func<TNode, object?>> value, string? format = null) {
		// Привязка всегда односторонняя
		Current.Binding = new Binding(PropertyUtil.GetName(value)) {
			Mode = BindingMode.OneWay,
			StringFormat = format
		};
		return this;
	}

	public ColumnsConfig<TNode> Width(double pixels) {
		Current.Width = new DataGridLength(pixels);
		return this;
	}

	/// <summary>
	/// Колонка делит с другими такими же свободную ширину таблицы в заданной Доле
	/// </summary>
	public ColumnsConfig<TNode> StarWidth(double weight = 1) {
		Current.Width = new DataGridLength(weight, DataGridLengthUnitType.Star);
		return this;
	}

	/// <summary>Ниже этой ширины долевая колонка не сжимается</summary>
	public ColumnsConfig<TNode> MinWidth(double pixels) {
		Current.MinWidth = pixels;
		return this;
	}

	// у числовых колонок разряды должны стоять друг под другом, прежимаем к правому краю
	public ColumnsConfig<TNode> RightAligned() {
		Current.CellStyleClasses.Add("number");
		return this;
	}

	public IJournalColumnsConfig Finish() => this;

	public DataGrid MakeTable() {
		var grid = new DataGrid {
			IsReadOnly = true,
			AutoGenerateColumns = false,
			HeadersVisibility = DataGridHeadersVisibility.Column,
			GridLinesVisibility = DataGridGridLinesVisibility.All,
			CanUserResizeColumns = true,
			// Строки грузятся страницами, и сортировка по заголовку переставила бы только
			// загруженную часть списка. Порядок задаёт запрос журнала.
			CanUserSortColumns = false
		};
		foreach(var column in columns)
			grid.Columns.Add(column);
		return grid;
	}
}
