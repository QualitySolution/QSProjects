using System;
using System.ServiceModel;
using NLog;

namespace QS.ErrorReporting
{
	public static class ReportWorker
	{
		public static String ServiceAddress = "http://saas.qsolution.ru:2048/ErrorReporting";

		public static IErrorReportingService GetReportService ()
		{
			return new HttpErrorReportingService(ServiceAddress);
		}
	}
}
