using System;
using System.Collections.Generic;
using QSProjectsLib;
using NLog;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace QSDocTemplates
{
	public abstract class DocParserBase<TDoc> : IDocParser
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public List<PatternField> Fields;

		public TDoc RootObject { get; set;}

		protected List<PatternField> fieldsList = new List<PatternField>();

		public List<PatternField> FieldsList
		{
			get
			{
				return fieldsList;
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
					logger.Warn(ex, "При получении значения поля {0}, произошло исключение NullReferenceException.");
				}
			}
			fieldsList.Add(field);
			FieldsHasValues = RootObject != null;
		}

		protected void AddField(Expression<Func<TDoc, object>> sourceProperty, PatternFieldType fieldType)
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
			AddField(sourceProperty, name, fieldType);
		}

		public void SortFields()
		{
			fieldsList.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
		}
	}
}

