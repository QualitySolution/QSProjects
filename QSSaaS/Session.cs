using System;
using System.ServiceModel.Web;
using System.ServiceModel;
using NLog;

namespace QSSaaS
{
	public class Session
	{
		public static String SaaSService = String.Empty;
		public static String SessionId = String.Empty;
		private Logger logger = LogManager.GetCurrentClassLogger ();

		public static void Refresh()
		{
			if (SaaSService == String.Empty) {
				logger.Error ("Не задан адрес сервиса!");
				return;
			}
			if (SessionId == String.Empty) {
				logger.Error ("Не задан ID сессии!");
				return;
			}
			try{
				Uri address = new Uri (SaaSService);
				var factory = new WebChannelFactory<ISaaSService> (new WebHttpBinding { AllowCookies = true }, address);
				ISaaSService svc = factory.CreateChannel ();
				if (!svc.refreshSession(SaaSService))
					logger.Warn("Не удалось продлить сессию " + SessionId + ".");
				factory.Close();
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка при продлении сессии " + SessionId + ".", ex);
			}
		}
	}
}

