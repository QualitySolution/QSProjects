using System;
using System.Data.Common;

namespace QSProjectsLib
{
	public partial class ChangePassword : Gtk.Dialog
	{
		public Mode WorkMode = Mode.DataBase;
		public string NewPassword;

		public enum Mode{DataBase, Manual};

		public ChangePassword ()
		{
			this.Build ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			NewPassword = entryPassword.Text;
			if(WorkMode == Mode.Manual)
				return;

			QSMain.OnNewStatusText("Отправляем новый пароль на сервер...");
			string sql;
			sql = "SET PASSWORD = PASSWORD('" + entryPassword.Text + "')";
			try 
			{
				DbCommand cmd = QSMain.ConnectionDB.CreateCommand();
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
				QSMain.OnNewStatusText("Пароль изменен. Ok");
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				QSMain.OnNewStatusText("Ошибка установки пароля!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnEntryPasswordChanged (object sender, EventArgs e)
		{
			bool CanSaveEmpty = entryPassword.Text != "" || entryPassword2.Text != "";
			bool CanSaveEqual = entryPassword.Text == entryPassword2.Text;
			bool CanSaveLength = entryPassword.Text.Length >= 3;
			bool CanSaveSpace = entryPassword.Text.IndexOf(' ') == -1;
			bool CanSaveCyrillic = !System.Text.RegularExpressions.Regex.IsMatch (entryPassword.Text, "\\p{IsCyrillic}");
			if(!CanSaveCyrillic) buttonOk.TooltipText = "Пароль не может содержать русские буквы";
			if(!CanSaveLength) buttonOk.TooltipText = "Пароль должен быть длиннее 2 символов";
			if(!CanSaveSpace) buttonOk.TooltipText = "Пароль не может содержать пробелов";
			if(!CanSaveEqual) buttonOk.TooltipText = "Оба введенных пароля должны совпадать";
			if(!CanSaveEmpty) buttonOk.TooltipText = "Сначала заполните оба поля";
			
			bool CanSave = CanSaveEmpty && CanSaveEqual && CanSaveLength && CanSaveSpace && CanSaveCyrillic;
			if(CanSave) buttonOk.TooltipText = "Сохранить новый пароль";
			buttonOk.Sensitive = CanSave;
		}
	}
}

