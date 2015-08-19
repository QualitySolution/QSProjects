using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GammaBinding.ColumnConfig
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

		public string GetColumnMappingString()
		{
			StringBuilder map = new StringBuilder ();
			map.Append ("{").Append (typeof(TNode).FullName).Append ("}");

			foreach(var column in Columns)
			{
				map.Append (" ").Append (column.DataPropertyName)
					.AppendFormat ("[{0}]{1};", column.Title, column.IsEditable ? "<>":"");
			}
			return map.ToString ();
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

