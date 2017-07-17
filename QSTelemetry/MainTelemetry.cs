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

        public static ITelemetryService GetTelemetryService()
		{
			try {
				var factory = new ChannelFactory<ITelemetryService>(new BasicHttpBinding(), ServiceAddress);
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
        public static bool IsDemo;
        #endregion

        static Timer IntervalSendTimer;

        public static void StartUpdateByTimer(int seconds)
        {
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
            if (Counters.ContainsKey(counter))
                Counters[counter] += val;
            else
                Counters[counter] = val;
        }

        public static void SendTelemetry()
        {
            var statistic = new TelemetryStatistic();
            statistic.UpdateStatisticId = SendedStatisticId;
            statistic.IsDemo = IsDemo;
            statistic.Product = Product;
            statistic.Edition = Edition;
            statistic.Version = Version;
            statistic.Counters = Counters;
            statistic.OS = Environment.OSVersion.VersionString;
            statistic.NetFramework = Environment.Version.ToString();

            var srv = GetTelemetryService();
            if (srv == null)
                return;
            var result = srv.SubmitStatistics(statistic);
            if (result > 0)
                SendedStatisticId = result;
        }

        static void HandleTimerCallback(object state)
        {

        }
    }
}
