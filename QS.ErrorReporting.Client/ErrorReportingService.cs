using System;
using Grpc.Core;

namespace QS.ErrorReporting
{
    public class ErrorReportingService: IDisposable, IErrorReportSender
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
        
        public static string ServiceAddress = "fail.cloud.qsolution.ru";
        public static int ServicePort = 4202;

        public ErrorReportingService()
        {
        }
        
        private Channel? channel;
        private Channel Channel {
            get {
                if(channel == null || channel.State == ChannelState.Shutdown)
                    channel = new Channel(ServiceAddress, ServicePort, ChannelCredentials.Insecure);
                if (channel.State == ChannelState.TransientFailure)
                    channel.ConnectAsync();
                return channel;
            }
        }
        
        #region Запросы
        
        public bool SubmitErrorReport(SubmitErrorRequest errorReport)
        {
            try
            {
                var client = new Reception.ReceptionClient(Channel);
                client.SubmitError(errorReport);
                return true;
            } catch (Exception e)
            {
                logger.Error(e, "Ошибка при отправке отчета об ошибке");
                return false;
            }
        }
        
        #endregion

        public void Dispose()
        {
            channel?.ShutdownAsync();
        }
    }
}
