using System;
using System.Collections.Generic;
using System.Data.Bindings;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using SIT.Components.ObjectComparer;

namespace QSHistoryLog
{
	public class HistoryChangeSet
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		public virtual int UserId { get; set; }
		public virtual string UserName { get; set; }
		public virtual DateTime ChangeTime { get; set; }
		public virtual ChangeSetType Operation { get; set; }
		public virtual string ObjectName { get; set; }
		public virtual int ItemId { get; set; }

		public virtual string ObjectTitle {
			get {
					var Descript = HistoryMain.ObjectsDesc.Find (d => d.ObjectName == ObjectName);
					return Descript != null ? Descript.DisplayName : ObjectName;
			}
		}

		string itemTitle;
		public virtual string ItemTitle {
			get {
					return itemTitle;
			}
			set {
				itemTitle = value;
			}
		}


		IList<FieldChange> changes;
		public virtual IList<FieldChange> Changes {
			get {
				if (changes == null)
					LoadChanges ();
				return changes;
			}
			set {
				changes = value;
			}
		}

		public string ChangeTimeText
		{
			get { return ChangeTime.ToString ("G");}
		}

		public string OperationText
		{
			get { return Operation.GetEnumTitle ();}
		}


		public HistoryChangeSet ()
		{
		}

		void LoadChanges()
		{
			logger.Debug("Получаем набор изменений...");
			MySqlCommand cmd = (MySqlCommand)QSMain.ConnectionDB.CreateCommand ();
			DBWorks.SQLHelper sql = new DBWorks.SQLHelper("SELECT history_changes.* FROM history_changes " +
			                                              "WHERE changeset_id = @changeset_id" );
			cmd.CommandText = sql.Text;
			cmd.Parameters.AddWithValue ("changeset_id", Id);

			using(MySqlDataReader rdr = cmd.ExecuteReader())
			{
				changes = new List<FieldChange> ();

				while (rdr.Read())
				{
					var change = new FieldChange {
						Id = rdr.GetInt32 ("id"),
						Path = rdr.GetString ("path"),
						Type = (ChangeType)Enum.Parse (typeof(ChangeType), rdr.GetString ("type")),
						OldValue = rdr.GetString ("old_value"),
						NewValue = rdr.GetString ("new_value")
					};

					changes.Add (change);
				}
			}
			logger.Debug(RusNumber.FormatCase (Changes.Count, "Загружен набор из {0} изменения.", "Загружен набор из {0} изменений.", "Загружен набор из {0} изменений."));
		}
	}
}

