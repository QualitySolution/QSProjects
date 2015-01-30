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
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public static void Refresh(Object StateInfo)
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
				if (!svc.refreshSession(SessionId))
					logger.Warn("Не удалось продлить сессию " + SessionId + ".");
				factory.Close();
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка при продлении сессии " + SessionId + ".", ex);
			}
		}
	}
}

