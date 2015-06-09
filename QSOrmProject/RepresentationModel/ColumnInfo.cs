using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gtk;

namespace QSOrmProject.RepresentationModel
{
	public interface IColumnInfo{
		string Name { get;}
		ColumnType Type { get;}

		object[] GetAttributes ();
	}

	public class ColumnInfo : IColumnInfo
	{
		public string Name { get; set;}

		public ColumnType Type { get; set;}

		Dictionary<string, int> Attributes = new Dictionary<string, int>();

		public ColumnInfo ()
		{
			
		}

		public ColumnInfo SetDataProperty<TVMNode> (Expression<Func<TVMNode, object>> propertyRefExpr)
		{
			switch(Type)
			{
			case ColumnType.Text:
				return SetAttributeProperty ("text", propertyRefExpr);
			}

			return this;
		}

		public ColumnInfo SetAttributeProperty<TVMNode> (string attibuteName, Expression<Func<TVMNode, object>> propertyRefExpr)
		{
			string nodePropertyName = PropertyUtil.GetPropertyNameCore (propertyRefExpr.Body);
			var prop = typeof(TVMNode).GetProperty (nodePropertyName);
			var att = prop.GetCustomAttributes (typeof(TreeNodeValueAttribute), true);
			if (att.Length == 0)
				throw new InvalidOperationException (String.Format ("Атрибут {2} в колонке {0} ссылается на свойство {1} для которого не задан атрибут TreeNodeValueAttribute.",
					Name,
					nodePropertyName,
					attibuteName
				));

			Attributes.Add (attibuteName, (att [0] as TreeNodeValueAttribute).Column);
			return this;
		}

		public object[] GetAttributes ()
		{
			object[] attributes = new object[Attributes.Count * 2];

			int idx = 0;
			foreach(KeyValuePair<string, int> item in Attributes)
			{
				attributes [idx] = item.Key;
				attributes [idx + 1] = item.Value;
				idx += 2;
			}
			return attributes;
		}
	}

	public enum ColumnType
	{
		Text
	}
}

