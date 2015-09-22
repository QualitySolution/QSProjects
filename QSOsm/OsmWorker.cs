using System;
using System.ServiceModel;
using NLog;
using QSOsm;
using System.ServiceModel.Web;

namespace QSOsm
{
	public static class OsmWorker
	{
		public static string ServiceAddress = "http://saas.qsolution.ru:2048/OsmService";

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public static IOsmService GetOsmService ()
		{
			try {
				Uri address = new Uri (ServiceAddress);
				var factory = new WebChannelFactory<IOsmService> (new WebHttpBinding { 
					AllowCookies = true,
					MaxReceivedMessageSize = 1048576
				}, address);
				return factory.CreateChannel ();
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка создания подключения к Osm сервису");
				return null;
			}
		}
	}
}

