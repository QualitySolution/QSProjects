using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using fyiReporting.RDL;

namespace QS.Report
{
	public class ReportInfo
	{
		#region Настройки отчета

		/// <summary>
		/// Техническое имя отчета
		/// </summary>
		public string Identifier { get; set; }

		/// <summary>
		/// Имя отчета для пользователя
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Словать с параметрами отчета
		/// </summary>
		public Dictionary<string, object> Parameters = new Dictionary<string, object>();

		/// <summary>
		/// Если заначение true тип DateTime будет передаваться в отчет вместе со временем.
		/// Если значение false то время будет обрезаться, в строковом представлении при передачи в отчет.
		/// </summary>
		public bool ParameterDatesWithTime { get; set; } = true;
		
		/// <summary>
		/// Запрещённые для экспорта форматы
		/// </summary>
		public OutputPresentationType[] RestrictedOutputPresentationTypes { get; set; }

		/// <summary>
		/// Указывается надо ли передвать серверу в ConnectionString параметр Allow User Variables=True
		/// </summary>
		public bool UseUserVariables { get; set; } = false;

		/// <summary>
		/// Строковое представление отчета в виде XML. Для передачи отчета через память без записи на диск.
		/// </summary>
		public string Source { get; set; }

		#endregion

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

		public Uri GetReportUri ()
		{
			return new Uri (GetPath ());
		}

		public string Path { get; set; }

		#region Обработка параметров
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
		#endregion

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

