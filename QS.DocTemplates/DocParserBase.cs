﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NLog;

namespace QS.DocTemplates
{
	public abstract class DocParserBase<TDoc> : IDocParser
		where TDoc : class
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public TDoc RootObject { get; set;}

		protected List<PatternField> fieldsList = new List<PatternField>();

		protected List<IPatternTable<TDoc>> tablesList = new List<IPatternTable<TDoc>>();
		protected List<IPatternTable> customTablesList = new List<IPatternTable>();

		public List<PatternField> FieldsList
		{
			get
			{
				return fieldsList;
			}
		}

		public IEnumerable<IPatternTableField> TablesFields
		{
			get
			{
				List<IPatternTableField> result = new List<IPatternTableField>();
				result.AddRange(tablesList.SelectMany(x => x.Colunms));
				result.AddRange(customTablesList.SelectMany(x => x.Colunms));
				return result.AsEnumerable();
			}
		}

		public bool FieldsHasValues { get; protected set;}

		public DocParserBase ()
		{
			
		}

		public abstract void UpdateFields();

		protected void AddField(Expression<Func<TDoc, object>> sourceProperty, string name, PatternFieldType fieldType)
		{
			var field = new PatternField();
			field.Name = name;
			field.Type = fieldType;
			if(RootObject != null)
			{
				try{
					field.Value = sourceProperty.Compile().Invoke(RootObject);
				}
				catch(NullReferenceException ex)
				{
					logger.Warn(ex, "При получении значения поля {0}, произошло исключение NullReferenceException.", name);
				}
			}
			fieldsList.Add(field);
			FieldsHasValues = RootObject != null;
		}

		protected void AddField(Expression<Func<TDoc, object>> sourceProperty, PatternFieldType fieldType)
		{
			var name = PatternField.GetFieldName(sourceProperty);
			AddField(sourceProperty, name, fieldType);
		}

		protected void AddField(Expression<Func<TDoc, object>> sourceProperty, Expression<Func<TDoc, object>> nameFromProperty, PatternFieldType fieldType)
		{
			var name = PatternField.GetFieldName(nameFromProperty);
			AddField(sourceProperty, name, fieldType);
		}

		protected PatternTable<TDoc, TRow> AddTable<TRow>(Expression<Func<TDoc, IList<TRow>>> collectionProperty)
		{
			var name = PatternTable<TDoc, TRow>.GetCollectionName(collectionProperty);
			return AddTable(name, collectionProperty);
		}

		protected PatternTable<TRow> AddCustomTable<TRow>(string name, IList<TRow> collectionProperty)
		{
			var table = new PatternTable<TRow>(name);

			try {
				table.DataRows = collectionProperty;
			} catch(NullReferenceException ex) {
				logger.Warn(ex, "При получении строк таблицы {0}, произошло исключение NullReferenceException.", name);
			}

			customTablesList.Add(table);
			return table;
		}

		protected PatternTable<TDoc, TRow> AddTable<TRow>(string name, Expression<Func<TDoc, IList<TRow>>> collectionProperty)
		{
			var table = new PatternTable<TDoc, TRow>(name)
			{
				RootObject = RootObject,
			};

			if(RootObject != null)
			{
				try{
					table.DataRows = collectionProperty.Compile().Invoke(RootObject);
				}
				catch(NullReferenceException ex)
				{
					logger.Warn(ex, "При получении строк таблицы {0}, произошло исключение NullReferenceException.", name);
				}
			}

			tablesList.Add(table);
			return table;
		}

		public void SortFields()
		{
			fieldsList.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
		}

		public void SetDocObject(object doc)
		{
			RootObject = doc as TDoc;
		}
	}
}

