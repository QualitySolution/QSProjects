using System;
using System.Collections.Generic;
using QS.Print;

namespace QS.Report
{
	public interface IPrintableRDLDocument : IPrintableDocument
	{
		ReportInfo GetReportInfo(string connectionString = null);
		Dictionary<object, object> Parameters { get; set; }
	}
}
