using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using QSProjectsLib;

namespace QSDocTemplates
{
	public class PatternTable<TDoc, TRow> : IPatternTable<TDoc>, IPatternDataTable<TRow>, IPatternDataTableInfo
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public string Name;

		public TDoc RootObject;

		public IList<TRow> DataRows { get; set;}

		public List<PatternTableFieldGeneric<TRow>> ColunmsGeneric { get; set;}

		public List<IPatternTableField> Colunms{
			get {
				return ColunmsGeneric.OfType<IPatternTableField>().ToList();
			}
		}

		#region IPatternDataTableInfo implementation

		public int DataRowsCount
		{
			get
			{
				if (DataRows == null)
					return 0;
				return DataRows.Count;
			}
		}

		#endregion

		public PatternTable(string tableName)
		{
			Name = tableName;
			ColunmsGeneric = new List<PatternTableFieldGeneric<TRow>>();
			ColunmsGeneric.Add(new PatternTableFieldGeneric<TRow>(this, null, String.Format("{0}.{1}", Name, "НомерСтроки"), PatternFieldType.FAutoRowNumber));
		}

		public PatternTable<TDoc, TRow> AddColumn(Expression<Func<TRow, object>> sourceProperty, string fieldName, PatternFieldType fieldType)
		{
			var field = new PatternTableFieldGeneric<TRow>(
				this,
				sourceProperty.Compile(),
				String.Format("{0}.{1}", Name, fieldName),
				fieldType
			);
			ColunmsGeneric.Add(field);
			return this;
		}

		public PatternTable<TDoc, TRow> AddColumn(Expression<Func<TRow, object>> sourceProperty, PatternFieldType fieldType)
		{
			var name = PatternField.GetFieldName(sourceProperty);
			return AddColumn(sourceProperty, name, fieldType);
		}

		public PatternTable<TDoc, TRow> AddColumn(Expression<Func<TRow, object>> sourceProperty, Expression<Func<TRow, object>> nameFromProperty, PatternFieldType fieldType)
		{
			var name = PatternField.GetFieldName(nameFromProperty);
			return AddColumn(sourceProperty, name, fieldType);
		}

		public static string GetCollectionName(Expression<Func<TDoc, IList<TRow>>> sourceProperty)
		{
			var propertyChain = PropertyChainFromExp.Get(sourceProperty);
			List<string> names = new List<string>();
			foreach(var prop in propertyChain)
			{
				var attrs = (DisplayAttribute[]) prop.GetCustomAttributes (typeof(DisplayAttribute), false);
				string namepart;
				if ((attrs != null) && (attrs.Length > 0))
					namepart = StringWorks.StringToPascalCase (attrs[0].GetName());
				else
					namepart = prop.Name;
				names.Add(namepart);
			}
			var name = String.Join(".", names);
			return name;
		}
	}

	public interface IPatternTable<TDoc>
	{
		List<IPatternTableField> Colunms { get; }
	}

	public interface IPatternDataTable<TRow>
	{
		IList<TRow> DataRows{ get;}
	}

	public interface IPatternDataTableInfo{
		int DataRowsCount { get;}
	}
}

