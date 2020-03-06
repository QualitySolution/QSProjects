using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gtk;
using NLog;
using NLog.Targets;
using QS.Dialog.GtkUI;
using QSProjectsLib;

namespace QSSupportLib
{
	public partial class ErrorMsg : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		List<Exception> AppExceptions = new List<Exception> ();
		string message;
		bool reportSent = false;

		protected string AppExceptionText
		{
			get{ return string.Join ("\n Следующее исключение:\n", AppExceptions.Select (ex => ex.ToString ()));}
		}

		private IErrorReportingSettings errorReportingSettings { get; }

		public ErrorMsg (Window parent, Exception ex, string userMessage, IErrorReportingSettings errorReportingSettings)
		{
			if (parent != null)
				this.Parent = parent;
			this.Build ();

			AppExceptions.Add (ex);
			message = userMessage;
			this.errorReportingSettings = errorReportingSettings ?? new DefaultErrorReportingSettings();
			labelUserMessage.LabelProp = userMessage;
			labelUserMessage.Visible = userMessage != "";
			OnExeptionTextUpdate ();
			buttonSendReport.Sensitive = false;
			entryEmail.Text = QSMain.User?.Email;

			this.SetPosition (WindowPosition.CenterOnParent);

			textviewDescription.Buffer.Changed += InputsChanged;
			entryEmail.Changed += InputsChanged;
		}

		public void AddAnotherException(Exception ex, string userMessage)
		{
			AppExceptions.Add (ex);
			OnExeptionTextUpdate ();
		}

		public void OnExeptionTextUpdate()
		{
			textviewError.Buffer.Text = AppExceptionText;
		}

		void InputsChanged (object sender, EventArgs e)
		{
			var regex = new Regex (@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
			buttonSendReport.Sensitive =
				(!String.IsNullOrWhiteSpace (textviewDescription.Buffer.Text) && (!errorReportingSettings.SendErrorRequestEmail || regex.IsMatch(entryEmail.Text)));
			if (sender is Gtk.Entry) {
				if (!regex.IsMatch (entryEmail.Text))
					(sender as Gtk.Entry).ModifyText (Gtk.StateType.Normal, new Gdk.Color (255, 0, 0));
				else
					(sender as Gtk.Entry).ModifyText (Gtk.StateType.Normal);
			}
		}

		protected void OnButtonCopyClicked (object sender, EventArgs e)
		{
			Clipboard clipboard = Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));

			string TextMsg = String.Format ("Продукт: {0}\nВерсия: {1}\nРедакция: {2}\nОшибка: {4}\n{3}", 
				                 MainSupport.ProjectVerion.Product, 
				                 MainSupport.ProjectVerion.Version,
				                 MainSupport.ProjectVerion.Edition,
								 AppExceptionText,
				                 message
			                 );
			clipboard.Text = TextMsg;
			clipboard.Store ();
		}

		protected void OnButtonSendReportClicked (object sender, EventArgs e)
		{
			string log = GetLog();
			SendReport(log);
		}

		private void SendReport(string logContent)
		{
			var svc = QSBugReporting.ReportWorker.GetReportService();
			if(svc == null) {
				MessageDialogHelper.RunErrorDialog("Не удалось установить соединение с сервером Quality Solution.");
				return;
			}

			if(!String.IsNullOrWhiteSpace(errorReportingSettings.MessageInfo)) {
				if(!String.IsNullOrWhiteSpace(textviewDescription.Buffer.Text))
					textviewDescription.Buffer.Text += Environment.NewLine;
				textviewDescription.Buffer.Text += errorReportingSettings.MessageInfo;
			}

			var result = svc.SubmitBugReport(
				new QSBugReporting.BugMessage {
					product = MainSupport.ProjectVerion.Product,
					Edition = MainSupport.ProjectVerion.Edition,
					version = MainSupport.ProjectVerion.Version.ToString(),
					stackTrace = String.Format("{0}{1}",
						String.IsNullOrWhiteSpace(message) ? String.Empty : String.Format("Пользовательское сообщение:{0}\n", message),
						AppExceptionText),
					description = textviewDescription.Buffer.Text,
					email = entryEmail.Text,
					userName = QSMain.User.Name,
					logFile = logContent
				});

			if(result) {
				this.Respond(ResponseType.Ok);
				reportSent = true;
			} else {
				MessageDialogHelper.RunWarningDialog("Отправка сообщения не удалась.\n" +
					"Проверьте ваше интернет соединение и повторите попытку. Если отправка неудастся возможно имеются проблемы на стороне сервера.");
			}

		}

		private string GetLogFilePath()
		{
			var fileTarget = LogManager.Configuration.AllTargets.FirstOrDefault(t => t is FileTarget) as FileTarget;
			return fileTarget == null ? string.Empty : fileTarget.FileName.Render(new LogEventInfo { Level = LogLevel.Debug });
		}

		private string GetShortLog(int rowCount)
		{
			string logFileName = GetLogFilePath();
			string logContent = String.Empty;

			if(String.IsNullOrWhiteSpace(logFileName))
				return String.Empty;
				
			try {
				string[] logs = System.IO.File.ReadAllLines(logFileName);
				if(logs.Length < rowCount)
					rowCount = logs.Length;
				logContent = string.Join(Environment.NewLine, logs.Skip(logs.Length - rowCount));
			} catch(Exception ex) {
				logger.Error(ex, "Не смогли прочитать лог файл {0}, для отправки.");
			}

			return logContent;
		}

		private string GetLog()
		{
			string logFileName = GetLogFilePath();
			string logContent = String.Empty;
			if(!String.IsNullOrWhiteSpace(logFileName)) {
				try {
					logContent = System.IO.File.ReadAllText(logFileName);
				} catch(Exception ex) {
					logger.Error(ex, "Не смогли прочитать лог файл {0}, для отправки.");
				}
			}

			return logContent;
		}

		protected override void OnDestroyed()
		{
			if(errorReportingSettings.SendAutomatically && !reportSent) {
				if(String.IsNullOrWhiteSpace(textviewDescription.Buffer.Text))
					textviewDescription.Buffer.Text = "Сообщение отправлено автоматически";
				string log = errorReportingSettings.LogRowCount != null
						? GetShortLog(errorReportingSettings.LogRowCount.Value)
						: GetLog();
				SendReport(log);
			}
		
			base.OnDestroyed();
		}
	}
}