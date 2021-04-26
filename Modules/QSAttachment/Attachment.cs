using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using Gdk;
using System.IO;

namespace QSAttachment
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Attachment : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static uint MaxFileNameLength = 120;
		private ListStore FilesStore;
		private List<int> deletedItems;
		public string TableName = "files";
		public string AttachToTable;
		public int ItemId = -1;

		private enum FilesCol
		{
			id,
			name,
			size,
			icon,
			file
		}

		public Attachment ()
		{
			this.Build ();

			deletedItems = new List<int> ();

			FilesStore = new ListStore (typeof(int), typeof(string), typeof(long), typeof(Pixbuf), typeof(byte[]));

			iconviewFiles.TextColumn = (int)FilesCol.name;
			iconviewFiles.PixbufColumn = (int)FilesCol.icon;

			iconviewFiles.Model = FilesStore;
		}

		public bool HasChanges {
			get {
				if (deletedItems.Count > 0)
					return true;
				foreach (object[] row in FilesStore) {
					if ((int)row [(int)FilesCol.id] == -1)
						return true;
				}
				return false;
			}
		}

		public IEnumerable<AttachedFile> AttachedFiles{
			get {
				foreach(object[] file in FilesStore)
				{
					yield return new AttachedFile {
						Id = (int)file [(int)FilesCol.id],
						Name = (string)file [(int)FilesCol.name]
					};
				}
			}
		}

		public void UpdateFileList (bool copy = false)
		{
			if (TableName == "" || AttachToTable == "" || ItemId < 0)
				return;
			logger.Info ("Загружаем список файлов для {0}<{1}>", AttachToTable, ItemId);
			string filefield = copy ? ", file " : "";
			string sql = String.Format ("SELECT id, name, size{1} FROM {0} WHERE item_group = @item_group AND item_id = @item_id",
			                            TableName, filefield);
			try {
				MySqlCommand cmd = new MySqlCommand (sql, (MySqlConnection)QSMain.ConnectionDB);
				cmd.Parameters.AddWithValue ("@item_group", AttachToTable);
				cmd.Parameters.AddWithValue ("@item_id", ItemId);
				FilesStore.Clear ();
				byte[] file = null;
				using (MySqlDataReader rdr = cmd.ExecuteReader ()) {
					while (rdr.Read ()) {
						if (copy) {
							file = new byte[rdr.GetInt64 ("size")];
							rdr.GetBytes (rdr.GetOrdinal ("file"), 0, file, 0, rdr.GetInt32 ("size"));
						}
						FilesStore.AppendValues (copy ? -1 : rdr.GetInt32 ("id"),
						                         rdr.GetString ("name"),
						                         rdr.GetInt64 ("size"),
						                         FileIconWorks.GetPixbufForFile (rdr.GetString ("name")),
						                         copy ? file : null
						);
					}
				}
				logger.Info ("Ок");
			} catch (Exception ex) {
				string mes = "Ошибка списка файлов!";
				logger.Error (ex, mes);
				throw new ApplicationException (mes, ex);
			}
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog ("Выберите файл для прикрепления...",
			                                                   (Gtk.Window)this.Toplevel,
			                                                   FileChooserAction.Open,
			                                                   "Отмена", ResponseType.Cancel,
			                                                   "Прикрепить", ResponseType.Accept);
			if ((ResponseType)Chooser.Run () == ResponseType.Accept) {
				Chooser.Hide ();
				logger.Info ("Чтение файла...");

				byte[] file;
				using (FileStream fs = new FileStream (Chooser.Filename, FileMode.Open, FileAccess.Read)) {
					using (MemoryStream ms = new MemoryStream ()) {
						fs.CopyTo (ms);
						file = ms.ToArray ();
					}
				}
				string fileName = System.IO.Path.GetFileName (Chooser.Filename);
				//При необходимости обрезаем имя файла.
				if(fileName.Length > MaxFileNameLength)
				{
					string ext = System.IO.Path.GetExtension (fileName);
					string name = System.IO.Path.GetFileNameWithoutExtension (fileName);
					fileName = String.Format ("{0}{1}",
					                          name.Remove ((int)MaxFileNameLength - ext.Length),
												ext);
				}

				FilesStore.AppendValues (-1,
					                     fileName,
				                         file.LongLength,
				                         FileIconWorks.GetPixbufForFile (Chooser.Filename),
				                         file
				);
				logger.Info ("Ok");
			}
			Chooser.Destroy ();
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			TreePath sel = iconviewFiles.SelectedItems [0];
			TreeIter iter;
			FilesStore.GetIter (out iter, sel);

			logger.Info ("Сохраняем временный файл...");
			byte[] file = (byte[])FilesStore.GetValue (iter, (int)FilesCol.file);
			if (file == null) {
				file = LoadFileFromDB ((int)FilesStore.GetValue (iter, (int)FilesCol.id));
			}
			string TempFilePath = System.IO.Path.Combine (System.IO.Path.GetTempPath (), (string)FilesStore.GetValue (iter, (int)FilesCol.name));
			System.IO.File.WriteAllBytes (TempFilePath, file);
			logger.Info ("Открываем файл во внешнем приложении...");
			System.Diagnostics.Process.Start (TempFilePath);
		}

		byte[] LoadFileFromDB (int id)
		{
			logger.Info ("Получаем файл id={0}", id);
			string sql = String.Format ("SELECT file, size FROM {0} WHERE id = @id",
			                            TableName);
			try {
				byte[] file;
				MySqlCommand cmd = new MySqlCommand (sql, (MySqlConnection)QSMain.ConnectionDB);
				cmd.Parameters.AddWithValue ("@id", id);
				using (MySqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read ();

					file = new byte[rdr.GetInt64 ("size")];
					rdr.GetBytes (rdr.GetOrdinal ("file"), 0, file, 0, rdr.GetInt32 ("size"));
					TreeIter iter;
					ListStoreWorks.SearchListStore (FilesStore, id, (int)FilesCol.id, out iter);
					FilesStore.SetValue (iter, (int)FilesCol.file, (object)file);
					FilesStore.SetValue (iter, (int)FilesCol.size, (object)rdr.GetInt64 ("size"));
				}
				logger.Info ("Ок");
				return file;
			} catch (Exception ex) {
				string mes = "Получения файла из базы!";
				logger.Error (ex, mes);
				throw new ApplicationException (mes, ex);
			}
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			TreePath sel = iconviewFiles.SelectedItems [0];
			TreeIter iter;
			FilesStore.GetIter (out iter, sel);

			FileChooserDialog fc =
				new FileChooserDialog ("Укажите путь для сохранения файла",
				                       (Gtk.Window)this.Toplevel,
				                       FileChooserAction.Save,
				                       "Отмена", ResponseType.Cancel,
				                       "Сохранить", ResponseType.Accept);

			fc.CurrentName = (string)FilesStore.GetValue (iter, (int)FilesCol.name);
			fc.Show (); 
			if (fc.Run () == (int)ResponseType.Accept) {
				fc.Hide ();
				logger.Info ("Сохраняем файл на диск...");
				byte[] file = (byte[])FilesStore.GetValue (iter, (int)FilesCol.file);
				if (file == null) {
					file = LoadFileFromDB ((int)FilesStore.GetValue (iter, (int)FilesCol.id));
				}
				System.IO.File.WriteAllBytes (fc.Filename, file);
			}
			fc.Destroy ();
			logger.Info ("Ок");
		}

		protected void OnIconviewFilesSelectionChanged (object sender, EventArgs e)
		{
			buttonOpen.Sensitive = buttonSave.Sensitive = buttonDelete.Sensitive =
				iconviewFiles.SelectedItems.Length == 1;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			TreePath sel = iconviewFiles.SelectedItems [0];
			TreeIter iter;
			FilesStore.GetIter (out iter, sel);

			if ((int)FilesStore.GetValue (iter, (int)FilesCol.id) > 0) {
				deletedItems.Add ((int)FilesStore.GetValue (iter, (int)FilesCol.id));
			}
			FilesStore.Remove (ref iter);
		}

		public void SaveChanges ()
		{
			MySqlTransaction trans = (MySqlTransaction)QSMain.ConnectionDB.BeginTransaction ();
			try {
				SaveChanges (trans);
				trans.Commit ();
			} catch (Exception ex) {
				trans.Rollback ();
				throw ex;
			}
		}

		public void SaveChanges (MySqlTransaction trans)
		{
			logger.Info ("Записывам изменения в списке файлов...");
			string sql = String.Format ("INSERT INTO {0} (name, item_group, item_id, size, file) " +
			             "VALUES (@name, @item_group, @item_id, @size, @file)", TableName);
			MySqlCommand cmd = new MySqlCommand (sql, (MySqlConnection)QSMain.ConnectionDB, trans);
			cmd.Parameters.AddWithValue ("@name", "");
			cmd.Parameters.AddWithValue ("@item_group", AttachToTable);
			cmd.Parameters.AddWithValue ("@item_id", ItemId);
			cmd.Parameters.AddWithValue ("@size", 0);
			cmd.Parameters.AddWithValue ("@file", null);
			cmd.Prepare ();
			foreach (object[] row in FilesStore) {
				if ((int)row [(int)FilesCol.id] > 0)
					continue;
				logger.Info ("Отправляем {0}...", row [(int)FilesCol.name]);
				byte[] file = (byte[])row [(int)FilesCol.file];
				cmd.Parameters ["@name"].Value = row [(int)FilesCol.name];
				cmd.Parameters ["@size"].Value = file.LongLength;
				cmd.Parameters ["@file"].Value = file;
				try {
					cmd.ExecuteNonQuery ();
				} catch (MySqlException ex) {
					if (ex.Number == 1153) {
						logger.Warn (ex, "Превышен максимальный размер пакета для передачи на сервер.");
						string Text = String.Format ("При передачи файла {0}, " +
						              "превышен максимальный размер пакета для передачи на сервер базы данных. " +
						              "Файл не будет сохранен. " +
						              "Это значение настраивается на сервере, по умолчанию для MySQL оно равняется 1Мб. " +
						              "Максимальный размер файла поддерживаемый программой составляет 16Мб, мы рекомендуем " +
						              "установить в настройках сервера параметр <b>max_allowed_packet=16M</b>. Подробнее о настройке здесь " +
						              "http://dev.mysql.com/doc/refman/5.6/en/packet-too-large.html", row [(int)FilesCol.name]);
						MessageDialog md = new MessageDialog ((Gtk.Window)this.Toplevel, DialogFlags.Modal,
						                                      MessageType.Error, 
						                                      ButtonsType.Ok, Text);
						md.Run ();
						md.Destroy ();
					} else if (ex.Number == 1118) {
						logger.Warn (ex, "Ошибка: превышена допустимая длина строки.");
						string Text = String.Format ("При передачи файла {0} была превышена максимально допустимая " +
						              "длина строки в БД MySQL. Файл не будет сохранен. Также эта ошибка может " +
						              "возникать при отсутствии возможности записи в log файл. В качестве возможного решения данной " +
						              "проблемы мы рекомендуем установить в настройках сервера следующие параметры: " +
						              "<b>\ninnodb_log_file_size = 499M" +
						              "\ninnodb_log_buffer_size = 499M</b>.",
						                             row [(int)FilesCol.name]);
						MessageDialog md = new MessageDialog ((Gtk.Window)this.Toplevel, DialogFlags.Modal,
						                                      MessageType.Error, 
						                                      ButtonsType.Ok, Text);
						md.Run ();
						md.Destroy ();
					} else
						throw ex;
				}
			}

			if (deletedItems.Count > 0) {
				logger.Info ("Удаляем удаленные файлы на сервере...");
				DBWorks.SQLHelper sqld = new DBWorks.SQLHelper ("DELETE FROM {0} WHERE id IN ", TableName);
				sqld.QuoteMode = DBWorks.QuoteType.SingleQuotes;
				sqld.StartNewList ("(", ", ");
				deletedItems.ForEach (delegate(int obj) {
					sqld.AddAsList (obj.ToString ());
				});
				sqld.Add (")");
				cmd = new MySqlCommand (sqld.Text, (MySqlConnection)QSMain.ConnectionDB, trans);
				cmd.ExecuteNonQuery ();
			}
		}

		protected void OnButtonScanClicked (object sender, EventArgs e)
		{
			GetFromScanner scanWin = new GetFromScanner ();
			scanWin.Show ();
			int result = scanWin.Run ();
			if (result == (int)ResponseType.Ok) {
				FilesStore.AppendValues (-1,
				                         scanWin.FileName,
				                         scanWin.File.LongLength,
				                         FileIconWorks.GetPixbufForFile (scanWin.FileName),
				                         scanWin.File
				);

			}
			scanWin.Destroy ();
		}

		protected void OnIconviewFilesItemActivated (object o, ItemActivatedArgs args)
		{
			buttonOpen.Click ();
		}

	}
}

