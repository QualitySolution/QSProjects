using System;
using Gtk;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationModelBase : IRepresentationModel
	{
		#region IRepresentationModel implementation

		public abstract void UpdateNodes ();

		public abstract Type NodeType { get; }

		public abstract Type ObjectType { get; }

		public NodeStore NodeStore { get; set;}

		private List<IColumnInfo> columns = new List<IColumnInfo> ();

		public List<IColumnInfo> Columns {
			get {
				return columns;
			}
		}

		#endregion

		public void SetRowAttribute<TVMNode> (string attibuteName, Expression<Func<TVMNode, object>> propertyRefExpr)
		{
			Columns.ConvertAll (c => (ColumnInfo) c)
				.ForEach ((ColumnInfo column) => column.SetAttributeProperty(attibuteName, propertyRefExpr));
		}

		public RepresentationModelBase ()
		{
		}
	}
}

