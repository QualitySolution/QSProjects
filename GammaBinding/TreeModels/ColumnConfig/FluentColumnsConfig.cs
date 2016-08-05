using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamma.ColumnConfig
{
	public class FluentColumnsConfig<TNode> : IColumnsConfig
	{

		List<ColumnMapping<TNode>> Columns = new List<ColumnMapping<TNode>>();

		RowMapping<TNode> row;

		public IEnumerable<IColumnMapping> ConfiguredColumns {
			get { return Columns.OfType<IColumnMapping> ();	}
		}

		public FluentColumnsConfig ()
		{
		}

		public static FluentColumnsConfig<TNode> Create()
		{
			return new FluentColumnsConfig<TNode> ();
		}

		public ColumnMapping<TNode> AddColumn(string title)
		{
			var column = new ColumnMapping<TNode> (this, title);
			Columns.Add (column);
			return column;
		}

		public RowMapping<TNode> RowCells()
		{
			row = new RowMapping<TNode> (this);
			return row;
		}

		public IColumnsConfig Finish()
		{
			return this;
		}
	}
}

