using System;
using System.Data.Common;
using NLog;
using QSSaaS;

namespace QSProjectsLib
{
	public partial class ChangePassword : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public Mode WorkMode = Mode.DataBase;
		private bool _canSetEmptyPassword = false;
		public string NewPassword;

		public enum Mode
		{
			DataBase,
			Manual}

		;

		public ChangePassword ()
		{
			this.Build ();
		}

		public bool CanSetEmptyPassword {
			get {
				return _canSetEmptyPassword;
			}
			set {
				_canSetEmptyPassword = value;
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
				if (svc == null || !svc.changePassword (Session.SessionId, NewPassword)) {
					logger.Error ("Ошибка установки пароля!");
					return;
				}
				logger.Info ("Пароль изменен.");
			} else {
				logger.Info ("Отправляем новый пароль на сервер...");
				string sql;
				sql = "SET PASSWORD = PASSWORD('" + entryPassword.Text + "')";
				try {
					QSMain.CheckConnectionAlive ();
					DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery ();
					logger.Info ("Пароль изменен. Ok");
				} catch (Exception ex) {
					logger.ErrorException ("Ошибка установки пароля!", ex);
					QSMain.ErrorMessage (this, ex);
				}
			}
		}

		protected void OnEntryPasswordChanged (object sender, EventArgs e)
		{
			bool CanSaveEmpty = _canSetEmptyPassword || entryPassword.Text != "" || entryPassword2.Text != "";
			bool CanSaveEqual = entryPassword.Text == entryPassword2.Text;
			bool CanSaveLength = _canSetEmptyPassword || entryPassword.Text.Length >= 3;
			bool CanSaveSpace = entryPassword.Text.IndexOf (' ') == -1;
			bool CanSaveCyrillic = !System.Text.RegularExpressions.Regex.IsMatch (entryPassword.Text, "\\p{IsCyrillic}");
			if (!CanSaveCyrillic)
				buttonOk.TooltipText = "Пароль не может содержать русские буквы";
			if (!CanSaveLength)
				buttonOk.TooltipText = "Пароль должен быть длиннее 2 символов";
			if (!CanSaveSpace)
				buttonOk.TooltipText = "Пароль не может содержать пробелов";
			if (!CanSaveEqual)
				buttonOk.TooltipText = "Оба введенных пароля должны совпадать";
			if (!CanSaveEmpty)
				buttonOk.TooltipText = "Сначала заполните оба поля";
			
			bool CanSave = CanSaveEmpty && CanSaveEqual && CanSaveLength && CanSaveSpace && CanSaveCyrillic;
			if (CanSave)
				buttonOk.TooltipText = "Сохранить новый пароль";
			buttonOk.Sensitive = CanSave;
		}
	}
}

