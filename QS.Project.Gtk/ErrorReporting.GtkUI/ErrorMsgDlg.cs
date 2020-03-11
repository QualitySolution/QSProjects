using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gtk;
using NLog;
using NLog.Targets;
using QS.Dialog.GtkUI;
using QS.Project.Domain;
using QS.Project.VersionControl;

namespace QS.ErrorReporting.GtkUI
{
	public partial class ErrorMsgDlg : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		IApplicationInfo application;
		UserBase user;
		List<Exception> AppExceptions = new List<Exception> ();
		bool reportSent;

		protected string AppExceptionText
		{
			get{ return string.Join ("\n Следующее исключение:\n", AppExceptions.Select (ex => ex.ToString ()));
			}
		}

		private IErrorReportingSettings errorReportingSettings { get; }

		private IDataBaseInfo databaseInfo { get; }

		public ErrorMsgDlg (Exception exception, IApplicationInfo application, UserBase user, IErrorReportingSettings errorReportingSettings, IDataBaseInfo dataBaseInfo = null)
		{
			this.Build ();

			this.application = application;
			this.user = user;

			this.errorReportingSettings = errorReportingSettings ?? throw new ArgumentNullException(nameof(errorReportingSettings));
			this.databaseInfo = dataBaseInfo;

			AppExceptions.Add (exception);
			OnExeptionTextUpdate ();
			buttonSendReport.Sensitive = false;

			entryEmail.Text = user?.Email;

			this.SetPosition (WindowPosition.CenterOnParent);

			textviewDescription.Buffer.Changed += InputsChanged;
			entryEmail.Changed += InputsChanged;
		}

		public void AddAnotherException(Exception exception)
		{
			AppExceptions.Add (exception);
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
				(!errorReportingSettings.RequestDescription || !String.IsNullOrWhiteSpace (textviewDescription.Buffer.Text) 
				&& (!errorReportingSettings.RequestEmail || regex.IsMatch(entryEmail.Text)));
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

			string TextMsg = String.Format ("Продукт: {0}\nВерсия: {1}\nРедакция: {2}\nОшибка: {3}", 
				                 application.ProductName, 
				                 application.Version,
				                 application.Edition,
								 AppExceptionText
			                 );
			clipboard.Text = TextMsg;
			clipboard.Store ();
		}

		protected void OnButtonSendReportClicked (object sender, EventArgs e)
		{
			string log = GetLog();
			SendReport(log, ErrorReportType.User);
		}

		private void SendReport(string logContent, ErrorReportType reportType)
		{
			var svc = ReportWorker.GetReportService();
			if(svc == null) {
				MessageDialogHelper.RunErrorDialog("Не удалось установить соединение с сервером Quality Solution.");
				return;
			}

			var result = svc.SubmitErrorReport(
				new ErrorReport {
					Product = application.ProductName,
					Edition = application.Edition,
					Version = application.Version.ToString(),
					DBName = databaseInfo?.Name ?? String.Empty,
					StackTrace = AppExceptionText,
					Description = textviewDescription.Buffer.Text,
					Email = entryEmail.Text,
					UserName = user?.Name,
					LogFile = logContent,
					ReportType = reportType
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
				string log = errorReportingSettings.LogRowCount != null
						? GetShortLog(errorReportingSettings.LogRowCount.Value)
						: GetLog();
				SendReport(log, ErrorReportType.Automatic);
			}

			base.OnDestroyed();
		}
	}
}