using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using QS.Cloud.Client;
using QS.Project.Versioning;

namespace QS.Cloud.Email.DesktopClient
{
	public class DesktopEmailSenderClient : CloudClientBySession
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static string ServiceAddress = "emailsender.cloud.qsolution.ru";
		public static int ServicePort = 443;

		private readonly IApplicationInfo applicationInfo;

		public DesktopEmailSenderClient(ISessionInfoProvider sessionInfoProvider, IApplicationInfo applicationInfo)
			: base(sessionInfoProvider, ServiceAddress, ServicePort)
		{
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
		}

		#region Запросы

		/// <summary>
		/// Отправляет одно письмо. ApplicationId заполняется автоматически из IApplicationInfo.
		/// </summary>
		public string SendMessage(string address, string subject, string text, IList<EmailAttachment>? attachments = null)
		{
			return SendMessages(new[] { new DesktopEmailMessage(address, subject, text, attachments) });
		}

		/// <summary>
		/// Отправляет несколько писем. ApplicationId заполняется автоматически из IApplicationInfo.
		/// </summary>
		public string SendMessages(IEnumerable<DesktopEmailMessage> messages, IProgress<int>? progress = default, CancellationToken token = default)
		{
			var client = new DesktopEmailSender.DesktopEmailSenderClient(Channel);
			using(var call = client.SendEmail(headers)) {
				int i = 1;
				foreach(var message in messages) {
					token.ThrowIfCancellationRequested();
					call.RequestStream.WriteAsync(BuildRequest(message)).Wait();
					progress?.Report(i);
					i++;
				}
				call.RequestStream.CompleteAsync().Wait();
				return call.GetAwaiter().GetResult().Results;
			}
		}

		/// <summary>
		/// Асинхронно отправляет несколько писем. ApplicationId заполняется автоматически из IApplicationInfo.
		/// </summary>
		public async Task<string> SendMessagesAsync(IEnumerable<DesktopEmailMessage> messages, IProgress<int>? progress = default, CancellationToken token = default)
		{
			var client = new DesktopEmailSender.DesktopEmailSenderClient(Channel);
			using(var call = client.SendEmail(headers)) {
				int i = 1;
				foreach(var message in messages) {
					token.ThrowIfCancellationRequested();
					await call.RequestStream.WriteAsync(BuildRequest(message));
					progress?.Report(i);
					i++;
				}
				await call.RequestStream.CompleteAsync();
				return (await call).Results;
			}
		}

		#endregion

		private SendEmailRequest BuildRequest(DesktopEmailMessage message)
		{
			var request = new SendEmailRequest {
				ApplicationId = applicationInfo.ProductCode,
				Messages = new EmailMessage {
					Address = message.Address,
					Subject = message.Subject,
					Text = message.Text,
				}
			};
			if(message.Attachments != null) {
				foreach(var attachment in message.Attachments) {
					request.Messages.Files.Add(new Attachment {
						File = Google.Protobuf.ByteString.CopyFrom(attachment.Bytes),
						FileName = attachment.FileName
					});
				}
			}
			return request;
		}
	}

	public class DesktopEmailMessage
	{
		public string Address { get; }
		public string Subject { get; }
		public string Text { get; }
		public IList<EmailAttachment>? Attachments { get; }

		public DesktopEmailMessage(string address, string subject, string text, IList<EmailAttachment>? attachments = null)
		{
			Address = address ?? throw new ArgumentNullException(nameof(address));
			Subject = subject ?? throw new ArgumentNullException(nameof(subject));
			Text = text ?? throw new ArgumentNullException(nameof(text));
			Attachments = attachments;
		}
	}

	public class EmailAttachment
	{
		public byte[] Bytes { get; }
		public string FileName { get; }

		public EmailAttachment(byte[] bytes, string fileName)
		{
			Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
			FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
		}
	}
}

