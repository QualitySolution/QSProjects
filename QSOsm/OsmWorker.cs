using System;
using System.ServiceModel;
using NLog;
using QSOsm;

namespace QSOsm
{
	public static class OsmWorker
	{
		public static string ServiceAddress = "http://saas.qsolution.ru:2048/OsmService";

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public static IOsmService GetReportService ()
		{
			try {
				var address = new Uri (ServiceAddress);
				var factory = new ChannelFactory<IOsmService> (new WebHttpBinding (), ServiceAddress);
				return factory.CreateChannel ();
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка создания подключения к сервису Osm.");
				return null;
			}
		}
	}
}

