using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace QS.ErrorReporting
{
    public class AsyncErrorReportingService: IDisposable, IAsyncErrorReportSender
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
        
        public static string ServiceAddress = "fail.cloud.qsolution.ru";
        public static int ServicePort = 4202;

        public AsyncErrorReportingService()
        {
        }
        
        private Channel? channel;

        private async Task<Channel> GetChannelAsync()
        {
	        if (channel == null || channel.State == ChannelState.Shutdown)
		        channel = new Channel(ServiceAddress, ServicePort, ChannelCredentials.Insecure);
	        if (channel.State == ChannelState.TransientFailure)
		        await channel.ConnectAsync();
	        return channel;
        }

        #region Запросы
        
        public async Task<bool> SubmitErrorReportAsync(SubmitErrorRequest errorReport)
        {
            try
            {
                var client = new Reception.ReceptionClient(await GetChannelAsync());
                await client.SubmitErrorAsync(errorReport);
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
