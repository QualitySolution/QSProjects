using System;
using QS.Print;

namespace QS.Report
{
	public interface IPrintableRDLDocument : IPrintableDocument
	{
		ReportInfo GetReportInfo();
	}
}
