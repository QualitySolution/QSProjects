using System;
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
		bool reportSent;

		protected string AppExceptionText => 
			string.Join("\n Следующее исключение:\n", errorReportingParameters.Exceptions.Select(ex => ex.ToString()));

		private IErrorReportingParameters errorReportingParameters { get; }
		private IErrorDialogSettings errorDialogSettings { get; }
		private IErrorReporter errorReporter { get; }

		public ErrorMsgDlg (IErrorReportingParameters errorReportingSettings, IErrorDialogSettings errorDialogSettings, IErrorReporter errorReporter)
		{
			this.Build();
			this.errorReportingParameters = errorReportingSettings ?? throw new ArgumentNullException(nameof(errorReportingSettings));
			this.errorDialogSettings = errorDialogSettings ?? throw new ArgumentNullException(nameof(errorDialogSettings));
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));

			OnExeptionTextUpdate();
			buttonSendReport.Sensitive = false;

			entryEmail.Text = errorReportingSettings.User?.Email;

			this.SetPosition (WindowPosition.CenterOnParent);

			textviewDescription.Buffer.Changed += InputsChanged;
			entryEmail.Changed += InputsChanged;
		}

		public void AddAnotherException(Exception exception)
		{
			errorReportingParameters.Exceptions.Add(exception);
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
								 errorReporter.ProductName,
								 errorReporter.Version,
								 errorReporter.Edition,
								 AppExceptionText
			                 );
			clipboard.Text = TextMsg;
			clipboard.Store();
		}

		protected void OnButtonSendReportClicked (object sender, EventArgs e)
		{
			errorReportingParameters.ReportType = ErrorReportType.User;
			errorReportingParameters.LogRowCount = null;
			if(!String.IsNullOrWhiteSpace(errorReportingParameters.Description) && !String.IsNullOrWhiteSpace(textviewDescription.Buffer.Text))
				errorReportingParameters.Description += "\n";
			errorReportingParameters.Description += textviewDescription.Buffer.Text;
			SendReport();
		}

		private void SendReport()
		{
			errorReportingParameters.Email = entryEmail.Text;
			var result = errorReporter.SendErrorReport(errorReportingParameters);
			if(result) {
				this.Respond(ResponseType.Ok);
				reportSent = true;
			} else {
				MessageDialogHelper.RunWarningDialog("Отправка сообщения не удалась.\n" +
				"Проверьте ваше интернет соединение и повторите попытку. Если отправка неудастся возможно имеются проблемы на стороне сервера.");
			}
		}

		protected override void OnDestroyed()
		{
			if(errorReportingParameters.CanSendAutomatically && !reportSent) {
				SendReport();
			}
			base.OnDestroyed();
		}
	}
}