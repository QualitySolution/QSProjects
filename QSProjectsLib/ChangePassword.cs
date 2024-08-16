using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using QS.Validation;
using QSSaaS;

namespace QSProjectsLib
{
	public partial class ChangePassword : Gtk.Dialog
	{
		private readonly IPasswordValidator passwordValidator;
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public Mode WorkMode = Mode.DataBase;
		public string NewPassword;

		public enum Mode
		{
			DataBase,
			Manual
		}

		public ChangePassword()
		{
			this.Build();
			passwordValidator = new PasswordValidator(new DefaultPasswordValidationSettings());
		}

		public ChangePassword(IPasswordValidator passwordValidator)
		{
			this.passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
			this.Build();
		}

		public bool CanSetEmptyPassword {
			get => passwordValidator.Settings.AllowEmpty;
			set {
				passwordValidator.Settings.AllowEmpty = value;
				OnEntryPasswordChanged (null, EventArgs.Empty);
			}
		}


		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			NewPassword = entryPassword.Text;
			if (WorkMode == Mode.Manual)
				return;
			if (Session.IsSaasConnection) {
				ISaaSService svc = Session.GetSaaSService ();
				if (svc == null || !svc.changeUserPasswordBySessionId (Session.SessionId, NewPassword)) {
					logger.Error ("Ошибка установки пароля!");
					return;
				}
				logger.Info ("Пароль изменен.");
			} else {
				logger.Info ("Отправляем новый пароль на сервер...");
				string sql;

				try {
					var reg = new Regex("(?i:password)=(.+?)(;|$)");
					QSMain.CheckConnectionAlive ();
					DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
					cmd.CommandText = "SELECT version();";
					var version = (string)cmd.ExecuteScalar();
					logger.Debug("Server version: " + version);
					if(version.EndsWith("-MariaDB"))
						sql = "SET PASSWORD = PASSWORD('" + entryPassword.Text + "')";
					else if(Version.Parse(version) >= new Version(8, 0, 13)) {
						var match = reg.Match(QSMain.ConnectionString);
						string oldPassword = match.Success ? match.Groups[1].Value : null;
						sql = $"SET PASSWORD='{entryPassword.Text}' REPLACE '{oldPassword}';";
					}
					else
						sql = "SET PASSWORD = PASSWORD('" + entryPassword.Text + "')";

					cmd.CommandText = sql;
					cmd.ExecuteNonQuery ();
					logger.Info ("Пароль изменен. Ok");

					//Заменяем пароль с текущей строке подключения, для того чтобы при переподключении не было ошибок 
					//и чтобы при смене пароля еще раз был текущий пароль.
					QSMain.ConnectionString = reg.Replace(QSMain.ConnectionString, $"password={entryPassword.Text};");

				} catch (Exception ex) {
					logger.Error (ex, "Ошибка установки пароля!");
					QSMain.ErrorMessage (this, ex);
				}
			}
		}

		protected void OnEntryPasswordChanged (object sender, EventArgs e)
		{
			var password1 = entryPassword.Text;
			var password2 = entryPassword2.Text;

			var result = passwordValidator.Validate(password1, out IList<string> errorMessages);

			if(password1 != password2) {
				errorMessages.Add("Пароли должны совпадать");
				result = false;
			}

			if(errorMessages.Count > 3) {
				labelValidationMessage.Markup = $"<span foreground=\"red\">{String.Join("\n", errorMessages.Take(3))}</span>";
			}
			else if (errorMessages.Any()) {
				labelValidationMessage.Markup = $"<span foreground=\"red\">{String.Join("\n", errorMessages)}</span>";
			}
			else {
				labelValidationMessage.Markup = "";
			}

			buttonOk.Sensitive = result;
		}
	}
}

