using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

