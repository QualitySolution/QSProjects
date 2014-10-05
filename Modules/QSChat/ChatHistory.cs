using System;
using System.Collections.Generic;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib;

namespace QSChat
{
	public partial class ChatHistory : Gtk.Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private TextTagTable textTags;
		private ListStore DatesList;

		public ChatHistory() :
		base(Gtk.WindowType.Toplevel)
		{
			this.Build();

			DatesList = new ListStore(typeof(DateTime));
			textTags = QSChatMain.BuildTagTable();
			treeviewDates.AppendColumn("Дата", new CellRendererText(), OnDateTreeCellDataFunc);
			UpdateDates();
			treeviewDates.Model = DatesList;
			treeviewDates.ShowAll();
			treeviewDates.Selection.Changed += OnDateListSelectionChanged;
		}

		void OnDateListSelectionChanged (object sender, EventArgs e)
		{
			if (treeviewDates.Selection.CountSelectedRows() != 1)
			{
				textviewTalks.Buffer.Clear();
				return;
			}
			TreeIter dateIter;
			treeviewDates.Selection.GetSelected(out dateIter);
			DateTime reqDate = (DateTime)DatesList.GetValue(dateIter, 0);
				
			logger.Info("Читаем диалог...");
			if((QSMain.ConnectionDB as MySqlConnection).State != System.Data.ConnectionState.Open)
			{
				logger.Warn("Соедиение с сервером не открыто.");
				return;
			}
			string sql = "SELECT datetime, text, users.name as user FROM chat_history " +
				"LEFT JOIN users ON users.id = chat_history.user_id " +
				"WHERE DATE(datetime) = @req_date " +
				"ORDER BY datetime";
			MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)QSMain.ConnectionDB);
			cmd.Parameters.AddWithValue("@req_date", reqDate);
			TextBuffer tempBuffer = new TextBuffer(textTags);
			TextIter iter = tempBuffer.EndIter;
			tempBuffer.InsertWithTagsByName(ref iter, String.Format("{0:D}", reqDate), "date");
			using (MySqlDataReader rdr = cmd.ExecuteReader())
			{
				while (rdr.Read())
				{
					DateTime mesDate = rdr.GetDateTime("datetime");
					tempBuffer.InsertWithTagsByName(ref iter, string.Format("\n({0:t}) {1}: ", mesDate, rdr.GetString("user")), 
						QSChatMain.GetUserTag(rdr.GetString("user")));
					tempBuffer.Insert(ref iter, rdr.GetString("text"));
				}
			}
			textviewTalks.Buffer = tempBuffer;
		}

		void OnDateTreeCellDataFunc (TreeViewColumn tree_column, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			DateTime date = (DateTime) tree_model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = String.Format("{0:D}", date);
		}

		protected void OnButtonCloseClicked(object sender, EventArgs e)
		{
			this.Destroy();
		}

		private void UpdateDates()
		{
			logger.Info("Обновляем даты...");
			if((QSMain.ConnectionDB as MySqlConnection).State != System.Data.ConnectionState.Open)
			{
				logger.Warn("Соедиение с сервером не открыто.");
				return;
			}
			string sql = "SELECT DISTINCT DATE(datetime) as cdate FROM chat_history " +
				"ORDER BY cdate";
			MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)QSMain.ConnectionDB);
			DatesList.Clear();
			using (MySqlDataReader rdr = cmd.ExecuteReader())
			{
				while (rdr.Read())
				{
					DatesList.AppendValues(rdr.GetDateTime("cdate"));
				}
			}
			logger.Info("Ok");
		}
	}
}

