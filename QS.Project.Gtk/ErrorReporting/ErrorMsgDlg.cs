using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;

namespace QS.ErrorReporting
{
	public partial class ErrorMsgDlg : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		List<Exception> AppExceptions = new List<Exception> ();
		bool reportSent;

		protected string AppExceptionText
		{
			get { return string.Join ("\n Следующее исключение:\n", AppExceptions.Select (ex => ex.ToString ())); }
		}

		private IErrorReportingSettings errorReportingSettings { get; }
		private IErrorDialogSettings errorDialogSettings { get; }
		private IErrorReporter errorReporter { get; }

		public ErrorMsgDlg (IErrorReportingSettings errorReportingSettings, IErrorDialogSettings errorDialogSettings, IErrorReporter errorReporter)
		{
			this.Build();
			this.errorReportingSettings = errorReportingSettings ?? throw new ArgumentNullException(nameof(errorReportingSettings));
			this.errorDialogSettings = errorDialogSettings ?? throw new ArgumentNullException(nameof(errorDialogSettings));
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));

			AppExceptions.Add(errorReportingSettings.Exception);
			OnExeptionTextUpdate();
			buttonSendReport.Sensitive = false;

			entryEmail.Text = errorReportingSettings.User?.Email;

			this.SetPosition (WindowPosition.CenterOnParent);

			textviewDescription.Buffer.Changed += InputsChanged;
			entryEmail.Changed += InputsChanged;
		}

		public void AddAnotherException(Exception exception)
		{
			AppExceptions.Add(exception);
			OnExeptionTextUpdate();
		}

		public void OnExeptionTextUpdate()
		{
			textviewError.Buffer.Text = AppExceptionText;
		}

		void InputsChanged (object sender, EventArgs e)
		{
			var regex = new Regex (@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
			buttonSendReport.Sensitive =
				(!errorDialogSettings.RequestDescription || !String.IsNullOrWhiteSpace (textviewDescription.Buffer.Text) 
				&& (!errorDialogSettings.RequestEmail || regex.IsMatch(entryEmail.Text)));
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
								 errorReporter.ApplicationInfo.ProductName,
								 errorReporter.ApplicationInfo.Version,
								 errorReporter.ApplicationInfo.Edition,
								 AppExceptionText
			                 );
			clipboard.Text = TextMsg;
			clipboard.Store ();
		}

		protected void OnButtonSendReportClicked (object sender, EventArgs e)
		{
			errorReportingSettings.ReportType = ErrorReportType.User;
			errorReportingSettings.LogRowCount = null;
			SendReport();
		}

		private void SendReport()
		{
			errorReportingSettings.Description = textviewDescription.Buffer.Text;
			errorReportingSettings.Email = entryEmail.Text;
			var res = errorReporter.SendErrorReport(errorReportingSettings);
			if(res) {
				this.Respond(ResponseType.Ok);
				reportSent = true;
			} else {
				MessageDialogHelper.RunWarningDialog("Отправка сообщения не удалась.\n" +
				"Проверьте ваше интернет соединение и повторите попытку. Если отправка неудастся возможно имеются проблемы на стороне сервера.");
			}
		}

		protected override void OnDestroyed()
		{
			if(errorReportingSettings.CanSendAutomatically && !reportSent) {
				SendReport();
			}
			base.OnDestroyed();
		}
	}
}