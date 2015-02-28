using System;
using QSProjectsLib;
using QSWidgetLib;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.Bindings;

namespace QSHistoryLog
{
	[System.ComponentModel.DisplayName("Просмотр журнала изменений")]
	public partial class HistoryView : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		List<HistoryChangeSet> ChangeSets;

		public HistoryView ()
		{
			this.Build ();
			datacomboObject.ItemsDataSource = HistoryMain.ObjectsDesc;
			comboAction.ItemsEnum = typeof(ChangeSetType);
			ComboWorks.ComboFillReference(comboUsers, "users", ComboWorks.ListMode.WithAll);
			selectperiod.ActiveRadio = SelectPeriod.Period.Today;
		}

		void UpdateJournal()
		{
			logger.Info("Получаем журнал изменений...");
			MySqlCommand cmd = (MySqlCommand)QSMain.ConnectionDB.CreateCommand ();
			DBWorks.SQLHelper sql = new DBWorks.SQLHelper("SELECT history_changeset.*, users.name as username FROM history_changeset " +
			                                              "LEFT JOIN users ON history_changeset.user_id = users.id " );
			if(entrySearch.Text != "")
				sql.Add ("LEFT JOIN history_changes ON history_changeset.id = history_changes.changeset_id ");

			sql.StartNewList (" WHERE ", " AND ");
			if(datacomboObject.SelectedItem is HistoryObjectDesc)
			{
				sql.AddAsList ("history_changeset.object_name = @object_name");
				cmd.Parameters.AddWithValue ("object_name", (datacomboObject.SelectedItem as HistoryObjectDesc).ObjectName);
			}

			if(ComboWorks.GetActiveId (comboUsers) > 0)
			{
				sql.AddAsList (" history_changeset.user_id = @user_id ");
				cmd.Parameters.AddWithValue ("user_id", ComboWorks.GetActiveId (comboUsers));
			}

			if (comboAction.SelectedItem is ChangeSetType) {
				sql.AddAsList (" history_changeset.operation = @operation");
				cmd.Parameters.AddWithValue ("operation", ((ChangeSetType)comboAction.SelectedItem).ToString("G"));
			}

			if(entrySearch.Text != "")
			{
				sql.AddAsList ("(history_changes.old_value LIKE @searchtext OR history_changes.new_value LIKE @searchtext)");
				cmd.Parameters.AddWithValue ("searchtext", String.Format ("%{0}%", entrySearch.Text));
			}

			if (!selectperiod.IsAllTime) {
				sql.AddAsList ("history_changeset.datetime BETWEEN @startdate AND @enddate");
				cmd.Parameters.AddWithValue ("startdate", selectperiod.DateBegin);
				cmd.Parameters.AddWithValue ("enddate", selectperiod.DateEnd);
			}

			logger.Debug (sql.Text);
			cmd.CommandText = sql.Text;

			using(MySqlDataReader rdr = cmd.ExecuteReader())
			{
				ChangeSets = new List<HistoryChangeSet> ();

				while (rdr.Read())
				{
					var changeset = new HistoryChangeSet () {
						Id = rdr.GetInt32 ("id"),
						UserId = DBWorks.GetInt (rdr, "user_id", -1),
						UserName = DBWorks.GetString (rdr, "username", "Неизвестный"),
						ChangeTime = rdr.GetDateTime ("datetime"),
						Operation = (ChangeSetType)Enum.Parse (typeof(ChangeSetType), rdr.GetString ("operation")),
						ObjectName = rdr.GetString ("object_name"),
						ObjectId = rdr.GetInt32 ("object_id"),
						ObjectTitle = DBWorks.GetString (rdr, "object_title", String.Empty)
					};

					ChangeSets.Add (changeset);
				}
			}
			datatreeChangesets.ItemsDataSource = ChangeSets;
			logger.Info(RusNumber.FormatCase (ChangeSets.Count, "Загружен {0} набор изменений.", "Загружено {0} набора изменений.", "Загружено {0} наборов изменений."));
		}

		protected void OnDatacomboObjectEnumItemSelected (object sender, QSOrmProject.EnumItemClickedEventArgs e)
		{
			UpdateJournal ();
		}

		protected void OnComboUsersChanged (object sender, EventArgs e)
		{
			UpdateJournal ();
		}

		protected void OnComboActionEnumItemSelected (object sender, QSOrmProject.EnumItemClickedEventArgs e)
		{
			UpdateJournal ();
		}

		protected void OnButtonSearchClicked (object sender, EventArgs e)
		{
			UpdateJournal ();
		}

		protected void OnSelectperiodDatesChanged (object sender, EventArgs e)
		{
			UpdateJournal ();
		}
	}
}

