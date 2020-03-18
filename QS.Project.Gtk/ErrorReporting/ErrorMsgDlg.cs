using System;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;

namespace QS.ErrorReporting
{
	public partial class ErrorMsgDlg : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
			
		public IErrorReporter ErrorReporter { get; }

		public ErrorMsgDlg (IErrorReporter errorReporter)
		{
			this.Build();
			this.ErrorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			
			buttonSendReport.Sensitive = errorReporter.CanSendReport;

			textviewError.Binding.AddBinding(errorReporter, er => er.ExceptionText, w => w.Buffer.Text).InitializeFromSource();
			textviewDescription.Binding.AddBinding(errorReporter, er => er.Description, w => w.Buffer.Text).InitializeFromSource();
			entryEmail.Binding.AddBinding(errorReporter, er => er.Email, w => w.Text).InitializeFromSource();

			this.SetPosition (WindowPosition.CenterOnParent);

			errorReporter.PropertyChanged += ErrorReporter_PropertyChanged;
		}

		void ErrorReporter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ErrorReporter.EmailIsValid)) {
				if (ErrorReporter.EmailIsValid)
					(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal);
				else
					(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal, new Gdk.Color(255, 0, 0));
			}
			if (e.PropertyName == nameof(ErrorReporter.CanSendReport))
				buttonSendReport.Sensitive = ErrorReporter.CanSendReport;
		}

		protected void OnButtonCopyClicked (object sender, EventArgs e)
		{
			Clipboard clipboard = Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));

			string TextMsg = String.Format ("Продукт: {0}\nВерсия: {1}\nРедакция: {2}\nОшибка: {3}",
								 ErrorReporter.ProductName,
								 ErrorReporter.Version,
								 ErrorReporter.Edition,
								 ErrorReporter.ExceptionText
			                 );
			clipboard.Text = TextMsg;
			clipboard.Store();
		}

		protected void OnButtonSendReportClicked (object sender, EventArgs e)
		{
			var result = ErrorReporter.SendErrorReport(ErrorReportType.User);
			if(result) {
				this.Respond(ResponseType.Ok);
			} else {
				MessageDialogHelper.RunWarningDialog("Отправка сообщения не удалась.\n" +
				"Проверьте ваше интернет соединение и повторите попытку. Если отправка неудастся возможно имеются проблемы на стороне сервера.");
			}
		}

		protected override void OnDestroyed()
		{
			ErrorReporter.SendErrorReport(ErrorReportType.Automatic);
			base.OnDestroyed();
		}
	}
}