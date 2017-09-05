using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QSProjectsLib;

namespace QSReport
{
	public class ReportInfo
	{
		public string Identifier { get; set; }

		public string Title { get; set; }

		public Dictionary<string, object> Parameters = new Dictionary<string, object>();

		public string GetPath ()
		{
			if (!String.IsNullOrWhiteSpace (Path))
				return Path;
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

		public string GetParametersString ()
		{
			if (Parameters == null)
				return String.Empty;
			var parametersBuild = new DBWorks.SQLHelper ();
			foreach (var param in Parameters) {
				string value;
				if (!(param.Value is string) && param.Value is IEnumerable)
					value = BuildMiltiValue (param.Value as IEnumerable);
				else
					value = ValueToValidString (param.Value);

				parametersBuild.AddAsList (String.Format ("{0}={1}", param.Key, value), "&");
			}
			return parametersBuild.Text;
		}

		private string BuildMiltiValue (IEnumerable values)
		{
			var valuesBuild = new DBWorks.SQLHelper ();

			foreach (var value in values) {
				valuesBuild.AddAsList (ValueToValidString (value), ",");
			}
			return valuesBuild.Text;
		}

		private string ValueToValidString (object value)
		{
			if (value == null)
				return String.Empty;
			if (value is DateTime)
				return ((DateTime)value).ToString ("O");
			return value.ToString ();
		}

		public bool UseUserVariables { get; set; } = false;

		public string ConnectionString {
			get {
				if (UseUserVariables)
					return QSMain.ConnectionString + ";Allow User Variables=True";
				return QSMain.ConnectionString;
			}
		}

		public ReportInfo ()
		{

		}
	}
}

