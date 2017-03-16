using System;
using System.Linq.Expressions;
using Gamma.Binding.Core.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSProjectsLib;

namespace QSDocTemplates
{
	public class PatternField : IPatternField
	{
		private static readonly string[] RemovedSimbols = new string[]{"-", "/", "(", ")"};

		public string Name { get; set;}
		public PatternFieldType Type { get; set;}
		public object Value;

		public PatternField()
		{

		}

		public PatternField(string name, PatternFieldType type)
		{
			Name = name;
			Type = type;
		}

		public static string GetFieldName<TRoot>(Expression<Func<TRoot, object>> sourceProperty)
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
				
				foreach (var symb in RemovedSimbols)
					namepart = namepart.Replace(symb, "");
				
				names.Add(namepart);
			}
			var name = String.Join(".", names);
			return name;
		}
	}

	public class PatternTableFieldGeneric<TRow> : PatternField, IPatternTableField
	{
		public IPatternDataTable<TRow> MyTable;

		public Func<TRow, object> GetValueFunc;

		public IPatternDataTableInfo DataTable { get{ return (IPatternDataTableInfo)MyTable; }}

		public object GetValue(int ix)
		{
			if (Type == PatternFieldType.FAutoRowNumber)
				return ix + 1;
			return GetValueFunc(MyTable.DataRows[ix]);
		}

		public PatternTableFieldGeneric(IPatternDataTable<TRow> table, Func<TRow, object> getValueFunc, string name, PatternFieldType type) : base(name, type)
		{
			MyTable = table;
			GetValueFunc = getValueFunc;
		}
	}

	public interface IPatternField{
		string Name { get;}
		PatternFieldType Type { get;}
	}

	public interface IPatternTableField : IPatternField
	{
		IPatternDataTableInfo DataTable{ get;}
		object GetValue(int ix);
	}

	public enum PatternFieldType{
		FString,
		FDate,
		FCurrency,
		FNumber,
		FAutoRowNumber
	}
}
