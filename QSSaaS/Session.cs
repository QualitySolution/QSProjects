using System;
using System.ServiceModel.Web;
using System.ServiceModel;
using NLog;

namespace QSSaaS
{
	public static class Session
	{
		public static String SaaSService = String.Empty;
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public static void Refresh(string session)
		{
			if (SaaSService == String.Empty) {
				logger.Error ("Не задан адрес сервиса!");
				return;
			}
			try{
				Uri address = new Uri (SaaSService);
				var factory = new WebChannelFactory<ISaaSService> (new WebHttpBinding { AllowCookies = true }, address);
				ISaaSService svc = factory.CreateChannel ();
				if (!svc.refreshSession(SaaSService))
					logger.Warn("Не удалось продлить сессию.");
				factory.Close();
			} catch (Exception ex) {
				logger.ErrorException ("Ошибка при продлении сессии.", ex);
			}
		}
	}
}

