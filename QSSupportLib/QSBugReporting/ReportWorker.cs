using System;
using System.ServiceModel;
using NLog;

namespace QSBugReporting
{
	[Obsolete("Здесь интерфейс отправки ошибок просто продублировани, на время доживания QSSupport, пока она не будет совсем удалена.")]
	public static class ReportWorker
	{
		public static String ServiceAddress = "http://saas.qsolution.ru:2048/BugReporting";

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public static IBugReportingService GetReportService ()
		{
			try {
				var factory = new ChannelFactory<IBugReportingService> (new BasicHttpBinding (), ServiceAddress);
				return factory.CreateChannel ();
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка создания подключения к сервису BugReporting.");
				return null;
			}
		}
	}
}

