﻿using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using NLog;

namespace QS.Osm
{
	public static class OsmWorker
	{
		public static string ServiceHost = "saas.qsolution.ru";
		public static int ServicePort = 2048;
		public static int MaxReceivedMessageSize = 1048576;

		public static string ServiceAddress{
			get{
				return $"http://{ServiceHost}:{ServicePort}/OsmService";
			}
		}

		static Logger logger = LogManager.GetCurrentClassLogger ();

		static WebChannelFactory<IOsmService> factory;

		public static IOsmService GetOsmService ()
		{
			try {
				if (factory == null || factory.State == CommunicationState.Closed) {
					Uri address = new Uri (ServiceAddress);
					factory = new WebChannelFactory<IOsmService>(new WebHttpBinding {
						AllowCookies = true,
						MaxReceivedMessageSize = OsmWorker.MaxReceivedMessageSize
					}, address);
				}
				return factory.CreateChannel ();
			} catch (Exception ex) {
				logger.Error (ex, "Ошибка создания подключения к Osm сервису");
				return null;
			}
		}
	}
}

