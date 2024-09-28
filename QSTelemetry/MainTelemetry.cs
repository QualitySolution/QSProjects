using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace QSTelemetry
{
    public static class MainTelemetry
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static String ServiceAddress = "http://saas.qsolution.ru:2048/Telemetry";
		public static bool SendingError = false;
		public static TimeSpan? SendTimeout;

        public static ITelemetryService GetTelemetryService()
		{
			try {
				var binding = new BasicHttpBinding();
				if (SendTimeout.HasValue)
					binding.SendTimeout = SendTimeout.Value;
				
				var factory = new ChannelFactory<ITelemetryService>(binding, ServiceAddress);
				return factory.CreateChannel();
			}
			catch (Exception ex) {
				logger.Error(ex, "Ошибка создания подключения к сервису Telemetry.");
				return null;
			}
		}

        static long? SendedStatisticId;
		static Dictionary<string, long> Counters = new Dictionary<string, long>();

        #region Внешние параметры
        public static string Product;
        public static string Edition;
        public static string Version;
        public static uint? ProductEdition;
        public static uint? EmployeesCount;
        public static bool IsDemo;

		public static bool DoNotTrack = false;
        #endregion

        static Timer IntervalSendTimer;

        public static void StartUpdateByTimer(int seconds)
        {
			if (DoNotTrack)
				return;

			var milliseconds = seconds * 1000;
            if(IntervalSendTimer != null)
            {
                IntervalSendTimer.Change(milliseconds, milliseconds);
            }
            else
            {
                IntervalSendTimer = new Timer(HandleTimerCallback, null, milliseconds, milliseconds);
            }
        }

        public static void AddCount(string counter, uint val = 1)
        {
			if (DoNotTrack)
				return;
			
			if (Counters.ContainsKey(counter))
                Counters[counter] += val;
            else
                Counters[counter] = val;
        }

        public static void SendTelemetry()
        {
			if (DoNotTrack)
				return;
			
			var statistic = new TelemetryStatistic();
            statistic.UpdateStatisticId = SendedStatisticId;
            statistic.IsDemo = IsDemo;
            statistic.Product = Product;
            statistic.Edition = Edition;
            statistic.Version = Version;
            statistic.ProductEdition = ProductEdition;
            statistic.BaseEmployees = EmployeesCount;
            statistic.Counters = Counters;
            statistic.OS = Environment.OSVersion.VersionString;
            statistic.NetFramework = Environment.Version.ToString();

			try
			{
				var srv = GetTelemetryService();
				if (srv == null)
					return;
				var result = srv.SubmitStatistics(statistic);
				if (result > 0)
					SendedStatisticId = result;
			}catch(Exception ex)
			{
				SendingError = true;
				logger.Warn(ex, "Ошибка при отправки телеметрии");
			}
        }

        static void HandleTimerCallback(object state)
        {
			SendTelemetry();
        }
    }
}
