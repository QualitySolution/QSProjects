using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using NLog;
using QSOrmProject;
using QSSupportLib;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Repositories;

namespace QSEmailSending
{
	public static class EmailManager
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static int MaxSendAttemptsCount = 5;
		private static int MaxEventSaveAttemptsCount = 5;


		private static CancellationTokenSource cancellationToken = new CancellationTokenSource();

		private static string userId = null;
		private static string userSecretKey = null;

		public static BlockingCollection<Email> emailsQueue = new BlockingCollection<Email>();
		public static BlockingCollection<MailjetEvent> unsavedEventsQueue = new BlockingCollection<MailjetEvent>();

		public static bool IsInitialized => !(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userSecretKey));

		static EmailManager()
		{
			//Установка всем не отправленным письмам из базы статус ошибки отправки в связи с остановкой сервера.
			Task.Run(() => SetErrorStatusWaitingToSendEmails());

			//Запуск воркеров по отправке писем
			Task.Run(() => ProcessEmailMailjet(), cancellationToken.Token);
			Task.Run(() => ProcessEmailMailjet(), cancellationToken.Token);
			Task.Run(() => ProcessEmailMailjet(), cancellationToken.Token);

			//Запуск воркера по пересохранению ошибочных событий
			Task.Run(() => ResaveEventWork());
		}

		public static void StopWorkers()
		{
			cancellationToken.Cancel();
		}

		public static void Init()
		{
			if(IsInitialized) {
				return;
			}

			var mailjetUserName = "MailjetUserId";
			var mailjetSecretName = "MailjetSecretKey";

			if(!MainSupport.BaseParameters.All.ContainsKey(mailjetUserName)
			   || !MainSupport.BaseParameters.All.ContainsKey(mailjetSecretName)) {
				logger.Error("В параметрах базы не указаны настройки подключения к серверу отправки почты Mailjet");
				return;
			}

			SetLoginSetting(MainSupport.BaseParameters.All[mailjetUserName], MainSupport.BaseParameters.All[mailjetSecretName]);
		}

		/// <summary>
		/// Устанавливает статус ошибки отправки, для всех писем в ожидании отправки.
		/// </summary>
		public static void SetErrorStatusWaitingToSendEmails()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()){
				logger.Info("Загрузка из базы почты ожидающей отправки");
				var waitingEmails = uow.Session.QueryOver<StoredEmail>()
									   .Where(x => x.State == StoredEmailStates.WaitingToSend)
									   .List<StoredEmail>();
				logger.Info("Добавление загруженной почты в очередь. Количество: {0}", waitingEmails.Count);
				foreach(var email in waitingEmails) {
					email.State = StoredEmailStates.SendingError;
					email.AddDescription("Отправка прервана остановкой сервера.");
					uow.Save(email);
				}
				uow.Commit();
			}
		}

		public static void AddEmailToSend(Email email)
		{
			logger.Info("Получено новое письмо на отправку");

			logger.Info("Запись в базу информации о письме");
			if(!EmailRepository.CanSendByTimeout(email.Recipient.EmailAddress, email.Order)) {
				logger.Info("Попытка отправить почту до истечения минимального времени до повторной отправки");
				throw new Exception("Отправка на один и тот же адрес возможна раз в 10 минут");
			}

			logger.Info("Запись в базу информации о письме");
			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<StoredEmail>()) {
				//Заполнение нового письма данными
				uow.Root.Order = uow.GetById<Order>(email.Order);
				uow.Root.DocumentType = email.OrderDocumentType;
				uow.Root.SendDate = DateTime.Now;
				uow.Root.StateChangeDate = DateTime.Now;
				uow.Root.HtmlText = email.HtmlText;
				uow.Root.Text = email.Text;
				uow.Root.Title = email.Title;
				uow.Root.State = StoredEmailStates.WaitingToSend;
				uow.Root.SenderName = email.Sender.Title;
				uow.Root.SenderAddress = email.Sender.EmailAddress;
				uow.Root.RecipientName = email.Recipient.Title;
				uow.Root.RecipientAddress = email.Recipient.EmailAddress;
				try {
					uow.Save();
					logger.Info("Информация о письме успешно сохранена");
				}
				catch(Exception ex) {
					logger.Info(string.Format("Ошибка при сохранении. Ошибка: {0}", ex.Message));
					throw ex;
				}
				email.StoredEmailId = uow.Root.Id;
				logger.Info("Добавление почты в очередь на отправку");
				Task.Run(() => emailsQueue.Add(email));
			}
		}

		public static async Task ProcessEmailMailjet()
		{
			while(true) {
				Email email = null;

				email = emailsQueue.Take();

				Thread.Sleep(1000);

				if(email == null) {
					return;
				}

				if(email.StoredEmailId == 0) {
					logger.Error("Не сохраненное письмо в очереди на отправку");
					AddEmailToSend(email);
					return;
				}

				logger.Info("Отправка письма из очереди");
				using(var uow = UnitOfWorkFactory.CreateForRoot<StoredEmail>(email.StoredEmailId)) {

					MailjetClient client = new MailjetClient(userId, userSecretKey) {
						Version = ApiVersion.V3_1,
					};

					//формируем письмо в формате mailjet для отправки
					logger.Info("Формирование запроса на сервер Mailjet");
					var request = CreateMailjetRequest(email);
					MailjetResponse response = null;
					try {
						logger.Info("Отправка запроса на сервер Mailjet");
						response = await client.PostAsync(request);
					}
					catch(Exception ex) {
						logger.Info(string.Format("Не удалось отправить письмо: {0}\n", ex.Message));
						SaveErrorInfo(uow, ex.Message);
						return;
					}

					MailjetMessage[] messages = response.GetData().ToObject<MailjetMessage[]>();

					logger.Info("Получен ответ: Code {0}", response.StatusCode);
					if(response.IsSuccessStatusCode) {
						uow.Root.State = StoredEmailStates.SendingComplete;
						foreach(var message in messages) {
							if(message.CustomID == uow.Root.Id.ToString()) {
								foreach(var messageTo in message.To) {
									if(messageTo.Email == email.Recipient.EmailAddress) {
										uow.Root.ExternalId = messageTo.MessageID.ToString();
									}
								}
							}
						}
						uow.Save();
						logger.Info(response.GetData());
					} else {
						switch(response.StatusCode) {

						//Unauthorized
						//Incorrect Api Key / API Secret Key or API key may be expired.
						case 401:
							Init();
							if(email.SendAttemptsCount >= MaxSendAttemptsCount) {
								SaveErrorInfo(uow, GetErrors(messages));
							} else {
								emailsQueue.Add(email);
							}
							break;

						//Too Many Requests
						//Reach the maximum number of calls allowed per minute.
						case 429:
							if(email.SendAttemptsCount >= MaxSendAttemptsCount) {
								SaveErrorInfo(uow, GetErrors(messages));
							} else {
								emailsQueue.Add(email);
							}
							break;

						//Internal Server Error
						case 500:
							SaveErrorInfo(uow, string.Format("Внутренняя ошибка сервера Mailjet: {0}", GetErrors(messages)));
							break;
						default:
							SaveErrorInfo(uow, GetErrors(messages));
							break;
						}

						logger.Info(response.GetData());
						logger.Info(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
					}
				}

			}
		}

		private static void SaveErrorInfo(IUnitOfWorkGeneric<StoredEmail> uow, string errorInfo)
		{
			uow.Root.State = StoredEmailStates.SendingError;
			uow.Root.AddDescription(errorInfo);
			uow.Save();
		}

		private static string GetErrors(MailjetMessage[] messages)
		{
			string errorResult = "";
			foreach(var message in messages) {
				foreach(var error in message.Errors) {
					if(!string.IsNullOrWhiteSpace(errorResult)) {
						errorResult += "\n";
					}
					errorResult += string.Format("StatusCode: {0}, ErrorCode: {1}, Error message: {2}", error.StatusCode, error.ErrorCode, error.ErrorMessage);
				}
			}
			return errorResult;
		}

		private static MailjetRequest CreateMailjetRequest(Email email)
		{
			MailjetRequest request = new MailjetRequest {
				Resource = Send.Resource
			};
			var attachments = new JArray();
			foreach(var item in email.AttachmentsBinary) {
				attachments.Add(new JObject{
					{"ContentType", "application/octet-stream"},
					{"Filename", item.Key},
					{"Base64Content", item.Value}
				});
			}
			var inlinedAttachments = new JArray();
			foreach(var item in email.InlinedAttachments) {
				inlinedAttachments.Add(new JObject{
					{"ContentID", item.Key},
					{"ContentType", item.Value.ContentType},
					{"Filename", item.Value.FileName},
					{"Base64Content", item.Value.Base64String}
				});
			}
			var message = new JObject {
				{"From", new JObject {
						{"Email", email.Sender.EmailAddress},
						{"Name", email.Sender.Title}
					}
				},
				{"To", new JArray {
						new JObject {
							{"Email", email.Recipient.EmailAddress},
							{"Name", email.Recipient.Title}
						}
					}
				},
				{"Subject", email.Title},
				{"TextPart", email.Text},
				{"HTMLPart", email.HtmlText},
				{"CustomID", email.StoredEmailId.ToString()},
				{"Attachments", attachments},
				{"InlinedAttachments", inlinedAttachments},
				{"TrackOpens", "account_default"},
				{"TrackClicks", "account_default"}
			};

			request.Property(Send.Messages, new JArray { message });

			return request;
		}

		public static void ProcessEvent(MailjetEvent mailjetEvent)
		{
			logger.Error("Событие с сервера Mailjet {0}\n", mailjetEvent);

			//Запись информации о письме в базу
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()){
				var emailAction = EmailRepository.GetStoredEmailByMessageId(uow, mailjetEvent.MessageID.ToString());
				if(emailAction == null) {
					int mailId;
					if(int.TryParse(mailjetEvent.CustomID, out mailId)) {
						emailAction = uow.GetById<StoredEmail>(mailId);
					}
				}
				if(emailAction != null) {
					var eventDate = UnixTimeStampToDateTime(mailjetEvent.Time);
					if(eventDate > emailAction.StateChangeDate) {
						emailAction.StateChangeDate = eventDate;
						switch(mailjetEvent.Event) {
						case "sent":
							emailAction.State = StoredEmailStates.Delivered;
							break;
						case "open":
							emailAction.State = StoredEmailStates.Opened;
							break;
						case "spam":
							emailAction.State = StoredEmailStates.MarkedAsSpam;
							break;
						case "bounce":
						case "blocked":
							emailAction.State = StoredEmailStates.Undelivered;
							emailAction.AddDescription(mailjetEvent.GetErrorInfo());
							break;
						}
						try {
							uow.Save(emailAction);
							uow.Commit();
							logger.Info("Письмо сохранено");
						}
						catch(Exception ex) {
							mailjetEvent.AttemptCount++;
							if(mailjetEvent.AttemptCount < MaxEventSaveAttemptsCount) {
								unsavedEventsQueue.Add(mailjetEvent);
							}
							logger.Error("Произошла ошибка при сохранении: {0}", ex.Message);
						}
					}
				}else {
					logger.Error("Не найдено письмо в БД связанное с событием с сервера Mailjet.");
				}
			}
		}

		private static void ResaveEventWork()
		{
			while(true) {
				MailjetEvent unsavedEvent = unsavedEventsQueue.Take();
				Task.Run(() => TryResaveEvent(unsavedEvent));
			}
		}

		private static void TryResaveEvent(MailjetEvent unsavedEvent)
		{
			//Попытка пересохранения каждые 20 сек.
			Thread.Sleep(20000);
			ProcessEvent(unsavedEvent);
		}


		private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dtDateTime;
		}

		private static void ClearLoginSetting()
		{
			userId = null;
			userSecretKey = null;
		}

		private static void SetLoginSetting(string id, string key)
		{
			if(IsInitialized) {
				return;
			}
			userId = id;
			userSecretKey = key;
		}

	}
}
