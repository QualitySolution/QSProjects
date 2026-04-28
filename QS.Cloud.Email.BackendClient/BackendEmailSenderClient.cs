using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QS.Cloud.Email.BackendClient
{
    public class BackendEmailSenderClient : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string ServiceAddress = "emailsender.cloud.qsolution.ru";
        public static int ServicePort = 443;

        private readonly Metadata headers;
        private Channel channel;

        public BackendEmailSenderClient(string backendToken) {
            if(string.IsNullOrWhiteSpace(backendToken))
                throw new ArgumentNullException(nameof(backendToken));
            headers = new Metadata { { "Authorization", $"Bearer {backendToken}" } };
        }

        protected Channel Channel {
            get {
                if(channel == null || channel.State == ChannelState.Shutdown)
                    channel = new Channel(ServiceAddress, ServicePort, new SslCredentials());
                if(channel.State == ChannelState.TransientFailure)
                    channel.ConnectAsync();
                return channel;
            }
        }

        #region Запросы

        /// <summary>
        /// Отправляет одно письмо.
        /// </summary>
        public virtual string SendEmail(string emailAddress, string title, string text)
        {
            return SendEmails(new[] { new BackendEmailMessage(emailAddress, title, text) });
        }

        /// <summary>
        /// Отправляет несколько писем.
        /// </summary>
        public virtual string SendEmails(IEnumerable<BackendEmailMessage> messages)
        {
            var client = new BackendEmailSender.BackendEmailSenderClient(Channel);
            var request = BuildRequest(messages);
            return client.SendEmail(request, headers).Results;
        }

        /// <summary>
        /// Асинхронно отправляет одно письмо.
        /// </summary>
        public virtual Task<string> SendEmailAsync(string emailAddress, string title, string text)
        {
            return SendEmailsAsync(new[] { new BackendEmailMessage(emailAddress, title, text) });
        }

        /// <summary>
        /// Асинхронно отправляет несколько писем.
        /// </summary>
        public virtual async Task<string> SendEmailsAsync(IEnumerable<BackendEmailMessage> messages)
        {
            var client = new BackendEmailSender.BackendEmailSenderClient(Channel);
            var request = BuildRequest(messages);
            var result = await client.SendEmailAsync(request, headers);
            return result.Results;
        }

        #endregion

        private SendEmailBackendRequest BuildRequest(IEnumerable<BackendEmailMessage> messages)
        {
            var request = new SendEmailBackendRequest();
            foreach(var message in messages) {
                request.Messages.Add(new OutgoingEmail {
                    EmailAddress = message.EmailAddress,
                    Title = message.Title,
                    Text = message.Text
                });
            }
            return request;
        }

        public void Dispose()
        {
            channel?.ShutdownAsync().Wait();
        }
    }

    public class BackendEmailMessage
    {
        public string EmailAddress { get; }
        public string Title { get; }
        public string Text { get; }

        public BackendEmailMessage(string emailAddress, string title, string text)
        {
            EmailAddress = emailAddress ?? throw new ArgumentNullException(nameof(emailAddress));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}

