using System;
using Gtk;
using NLog;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using Gdk;

namespace QSAttachment
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class Attachment : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ListStore FilesStore;
		public string TableName = "files";
		public string AttachToTable;
		public int ItemId = -1;
		private Pixbuf fileIcon;

		private enum FilesCol{
			id,
			name,
			size,
			icon
		}

		public Attachment()
		{
			this.Build();

			FilesStore = new ListStore ( typeof(int), typeof(string), typeof(long), typeof(Pixbuf) );

			iconviewFiles.TextColumn = (int)FilesCol.name;
			iconviewFiles.PixbufColumn = (int)FilesCol.icon;

			iconviewFiles.Model = FilesStore;

			fileIcon = Icon ("gnome-fs-regular");
		}

		public void UpdateFileList()
		{
			if (TableName == "" || AttachToTable == "" || ItemId < 0)
				return;
			logger.Info ("Загружаем список файлов для {0}<{1}>", AttachToTable, ItemId);
			string sql = String.Format("SELECT id, name, size FROM {0} WHERE item_group = @item_group, item_id = @item_id",
				TableName);
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)QSMain.ConnectionDB);
				cmd.Parameters.AddWithValue("@item_group", AttachToTable);
				cmd.Parameters.AddWithValue("@item_id", ItemId);
				FilesStore.Clear();
				using (MySqlDataReader rdr = cmd.ExecuteReader ()) 
				{
					while(rdr.Read ())
					{
						FilesStore.AppendValues(rdr.GetInt32("id"),
							rdr.GetString("name"),
							rdr.GetInt64("size"),
							fileIcon
						);
					}
				}
				logger.Info ("Ок");
			}catch (Exception ex)
			{
				string mes = "Ошибка чтения данных для настраиваемых полей!";
				logger.ErrorException(mes, ex);
				throw new ApplicationException (mes, ex);
			}


		}
	}
}

