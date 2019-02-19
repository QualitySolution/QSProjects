using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QS.Updater
{
	public static class UpdateService
	{
		public static Uri ServiceUrl = new Uri("http://saas.qsolution.ru:2048/Updater");

		public static IUpdateService GetService()
		{
			return new WebChannelFactory<IUpdateService>(new WebHttpBinding { AllowCookies = true }, ServiceUrl)
					.CreateChannel();
		}
	}
}
