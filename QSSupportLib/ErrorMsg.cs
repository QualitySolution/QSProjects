using System;
using Gtk;
using QSProjectsLib;
using System.Text.RegularExpressions;

namespace QSSupportLib
{
	public partial class ErrorMsg : Gtk.Dialog
	{
		Exception AppExpeption;
		string message;

		public ErrorMsg (Window parent, Exception ex, string userMessage)
		{
			if (parent != null)
				this.Parent = parent;
			this.Build ();

			AppExpeption = ex;
			message = userMessage;
			labelUserMessage.LabelProp = userMessage;
			labelUserMessage.Visible = userMessage != "";
			textviewError.Buffer.Text = ex.ToString ();
			buttonSendReport.Sensitive = false;
			//TODO FIXME Вставить email из пользователя.

			this.SetPosition (WindowPosition.CenterOnParent);

			textviewDescription.Buffer.Changed += InputsChanged;
			entryEmail.Changed += InputsChanged;
		}

		void InputsChanged (object sender, EventArgs e)
		{
			var regex = new Regex (@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
			buttonSendReport.Sensitive = 
				(!String.IsNullOrWhiteSpace (textviewDescription.Buffer.Text) && regex.IsMatch (entryEmail.Text));
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
				                 AppExpeption,
				                 message
			                 );
			clipboard.Text = TextMsg;
			clipboard.Store ();
		}

		protected void OnButtonSendReportClicked (object sender, EventArgs e)
		{
			var svc = QSBugReporting.ReportWorker.GetReportService ();
			var result = svc.SubmitBugReport (MainSupport.ProjectVerion.Product,
				             MainSupport.ProjectVerion.Version.ToString (), 
				             String.Format ("{0}\n{1}", AppExpeption, message), 
				             textviewDescription.Buffer.Text, 
				             entryEmail.Text);
			if (result) {
				this.Respond (Gtk.ResponseType.Ok);
			} else
				MessageDialogWorks.RunWarningDialog ("Отправка сообщения не удалась.\n" +
				"Проверьте ваше интернет соединение или повторите попытку позднее.");
		}
	}
}