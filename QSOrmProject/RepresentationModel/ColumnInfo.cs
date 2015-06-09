using System;
using System.Linq.Expressions;
using Gtk;

namespace QSOrmProject.RepresentationModel
{
	public interface IColumnInfo{
		string Name { get;}
		string NodePropertyName { get;}
		int DataColumn { get;}
		ColumnType Type { get;}
	}

	public class ColumnInfo : IColumnInfo
	{
		public string Name { get; set;}

		public string NodePropertyName { get; set;}

		public ColumnType Type { get; set;}

		public int DataColumn { get; set;}

		public ColumnInfo ()
		{
			
		}

		public ColumnInfo SetNodeProperty<TVMNode> (Expression<Func<TVMNode, object>> propertyRefExpr)
		{
			NodePropertyName = PropertyUtil.GetPropertyNameCore (propertyRefExpr.Body);
			var prop = typeof(TVMNode).GetProperty (NodePropertyName);
			var att = prop.GetCustomAttributes (typeof(TreeNodeValueAttribute), true);
			if (att.Length == 0)
				throw new InvalidOperationException (String.Format ("Колонка {0} ссылается на свойство {1} для которого не задан атрибут TreeNodeValueAttribute.",
						Name,
						NodePropertyName
					));
			DataColumn = (att [0] as TreeNodeValueAttribute).Column;

			return this;
		}
	}

	public enum ColumnType
	{
		Text
	}
}

