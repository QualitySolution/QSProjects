using System;
using System.ServiceModel.Web;
using System.ServiceModel;
using NLog;

namespace QSBugReporting
{
	public static class ReportWorker
	{
		public static String ServiceAddress = "http://saas.qsolution.ru:2048/BugReporting";

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public static IBugReportingService GetReportService ()
		{
			try {
				var address = new Uri (ServiceAddress);
				var factory = new WebChannelFactory<IBugReportingService> (new WebHttpBinding { AllowCookies = true }, address);
				return factory.CreateChannel ();
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка создания подключения к сервису BugReporting.");
				return null;
			}
		}
	}
}

