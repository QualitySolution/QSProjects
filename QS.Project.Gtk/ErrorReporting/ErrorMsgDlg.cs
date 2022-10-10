using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gtk;
using NLog;
using NLog.Targets;
using QS.Dialog.GtkUI;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Versioning;

namespace QS.ErrorReporting
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
			OnExceptionTextUpdate ();
			buttonSendReport.Sensitive = false;

			entryEmail.Text = user?.Email;

			this.SetPosition (WindowPosition.CenterOnParent);

			textviewDescription.Buffer.Changed += InputsChanged;
			entryEmail.Changed += InputsChanged;
		}

		public void AddAnotherException(Exception exception)
		{
			AppExceptions.Add (exception);
			OnExceptionTextUpdate ();
		}

		public void OnExceptionTextUpdate()
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
				                 application.Modification,
								 AppExceptionText
			                 );
			clipboard.Text = TextMsg;
			clipboard.Store ();
		}

		protected void OnButtonSendReportClicked (object sender, EventArgs e)
		{
			string log = GetLog();
			SendReport(log, ReportType.User);
		}

		private void SendReport(string logContent, ReportType reportType)
		{
			using (var reportingService = new ErrorReportingService())
			{
				var result = reportingService.SubmitErrorReport(
					new SubmitErrorRequest {
						App = new AppInfo{ 
							ProductCode = application.ProductCode,
							ProductName = application.ProductName ?? String.Empty,
							Modification= application.Modification ?? String.Empty,
							Version = application.Version.ToString(),
						},
						Db = new DatabaseInfo {
							Name = databaseInfo?.Name ?? String.Empty,
						},
						User = new UserInfo {
							Email = entryEmail.Text,
							Name = user?.Name ?? String.Empty,
						},
						Report = new ErrorInfo {
							StackTrace = AppExceptionText,
							UserDescription = textviewDescription.Buffer.Text,
							Log = logContent,
						},
						ReportType = reportType
					});

				if(result) {
					this.Respond(ResponseType.Ok);
					reportSent = true;
				} else {
					MessageDialogHelper.RunWarningDialog("Отправка сообщения не удалась.\n" +
					"Проверьте ваше интернет соединение и повторите попытку. Если отправка не удастся возможно имеются проблемы на стороне сервера.");
				}
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
				SendReport(log, ReportType.Automatic);
			}

			base.OnDestroyed();
		}
	}
}