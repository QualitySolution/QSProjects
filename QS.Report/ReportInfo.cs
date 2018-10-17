using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QS.Report
{
	public class ReportInfo
	{
		public string Identifier { get; set; }

		public string Title { get; set; }

		public Dictionary<string, object> Parameters = new Dictionary<string, object>();

		/// <summary>
		/// Если заначение true тип DateTime будет передаваться в отчет вместе со временем.
		/// Если значение false то время будет обрезаться, в строковом представлении при передачи в отчет.
		/// </summary>
		public bool ParameterDatesWithTime { get; set; } = true;

		public string GetPath ()
		{
			if (!String.IsNullOrWhiteSpace (Path))
				return Path;

			if(String.IsNullOrEmpty(Identifier))
				return null;

			var splited = Identifier.Split ('.').ToList ();
			var ReportName = splited.Last ();
			splited.RemoveAt (splited.Count - 1);
			var parts = new List<string> ();
			parts.Add (System.IO.Directory.GetCurrentDirectory ());
			parts.Add ("Reports");
			parts.AddRange (splited);
			parts.Add (ReportName + ".rdl");
			return System.IO.Path.Combine (parts.ToArray ());
		}

		/// <summary>
		/// Строковое представление отчета в виде XML. Для передачи отчета через память без записи на диск.
		/// </summary>
		public string Source { get; set; }

		public Uri GetReportUri ()
		{
			return new Uri (GetPath ());
		}

		public string Path { get; set; }

		public string GetParametersString ()
		{
			if (Parameters == null)
				return String.Empty;
			var parametersList = Parameters
			.Select(param => MakeParameterString(param.Key, param.Value));

			return String.Join("&", parametersList);
		}

		private string MakeParameterString(string key, object value)
		{
			string valueStr;
			if(!(value is string) && value is IEnumerable)
				valueStr = BuildMiltiValue(value as IEnumerable);
			else
				valueStr = ValueToValidString(value);

			return String.Format("{0}={1}", key, valueStr);
		}

		private string BuildMiltiValue (IEnumerable values)
		{
			var valuesList = values.Cast<object>().Select(ValueToValidString);
			return String.Join(",", valuesList);
		}

		private string ValueToValidString (object value)
		{
			if (value == null)
				return String.Empty;
			if (value is DateTime)
			{
				if(ParameterDatesWithTime)
					return ((DateTime)value).ToString ("O");
				else
					return ((DateTime)value).ToString("O").Substring(0,10);
			}
			return value.ToString ();
		}

		public bool UseUserVariables { get; set; } = false;

		public string ConnectionString {
			get {
				if (UseUserVariables)
					return Project.DB.Connection.ConnectionString + ";Allow User Variables=True";
				return Project.DB.Connection.ConnectionString;
			}
		}

		public ReportInfo ()
		{

		}
	}
}

