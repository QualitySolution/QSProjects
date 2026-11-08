using System;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Versioning;

namespace QS.ErrorReporting {
	public partial class ErrorMsgDlg : Gtk.Dialog {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly ErrorReportViewModel viewModel;

		public ErrorMsgDlg(Exception exception, IApplicationInfo application, UserBase user,
			IErrorReportingSettings settings, ILogService logService, IDataBaseInfo database = null) {
			Build();
			viewModel = new ErrorReportViewModel(exception, application, user, settings, logService, database);
			entryEmail.Text = viewModel.Email;
			textviewError.Buffer.Text = viewModel.ExceptionText;
			SetPosition(WindowPosition.CenterOnParent);
			textviewDescription.Buffer.Changed += InputsChanged;
			entryEmail.Changed += InputsChanged;
			UpdateInputs();
		}

		public void AddAnotherException(Exception exception) {
			viewModel.AddException(exception);
			textviewError.Buffer.Text = viewModel.ExceptionText;
		}

		private void InputsChanged(object sender, EventArgs e) {
			viewModel.Description = textviewDescription.Buffer.Text;
			viewModel.Email = entryEmail.Text;
			UpdateInputs();
		}

		private void UpdateInputs() {
			buttonSendReport.Sensitive = viewModel.CanSend;
			if(!String.IsNullOrWhiteSpace(viewModel.Email) && !viewModel.IsEmailValid)
				entryEmail.ModifyText(StateType.Normal, new Gdk.Color(255, 0, 0));
			else
				entryEmail.ModifyText(StateType.Normal);
		}

		protected void OnButtonCopyClicked(object sender, EventArgs e) {
			var clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
			clipboard.Text = viewModel.CopyText;
			clipboard.Store();
		}

		protected void OnButtonSendReportClicked(object sender, EventArgs e) {
			try {
				if(viewModel.Submit(ReportType.User)) {
					Respond(ResponseType.Ok);
					return;
				}
			} catch(Exception ex) { logger.Error(ex, "Не удалось отправить отчёт об ошибке"); }
			MessageDialogHelper.RunWarningDialog("Отправка сообщения не удалась.\nПроверьте ваше интернет соединение и повторите попытку. Если отправка не удастся возможно имеются проблемы на стороне сервера.");
		}

		protected override void OnDestroyed() {
			try {
				if(viewModel.SendAutomatically && !viewModel.ReportSent && !viewModel.Submit(ReportType.Automatic))
					logger.Warn("Автоматический отчёт об ошибке отправить не удалось");
			} catch(Exception ex) { logger.Error(ex, "Не удалось автоматически отправить отчёт об ошибке"); }
			base.OnDestroyed();
		}
	}
}
