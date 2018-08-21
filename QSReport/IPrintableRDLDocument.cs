using System;
using QS.Print;

namespace QSReport
{
	public interface IPrintableRDLDocument : IPrintableDocument
	{
		ReportInfo GetReportInfo();
	}
}
