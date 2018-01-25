using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using MySql.Data.MySqlClient;
using QSOrmProject;
using QSProjectsLib;

namespace QSHistoryLog.Domain
{
	public class HistoryChangeSet : IDomainObject
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

		public virtual string ChangeTimeText
		{
			get { return ChangeTime.ToString ("G");}
		}

		public virtual string OperationText
		{
			get { return Operation.GetEnumTitle ();}
		}


		public HistoryChangeSet ()
		{
		}

		public HistoryChangeSet(ChangeSetType operation, Type itemType, int itemId, string title)
		{
			Operation = operation;
			ChangeTime = DateTime.Now;
			UserId = QSMain.User.Id;
			ObjectName = itemType.Name;
			ItemId = itemId;
			ItemTitle = title;
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
						Type = (FieldChangeType)Enum.Parse (typeof(FieldChangeType), rdr.GetString ("type")),
						OldValue = rdr.GetString ("old_value"),
						NewValue = rdr.GetString ("new_value")
					};

					changes.Add (change);
				}
			}
			logger.Debug(RusNumber.FormatCase (Changes.Count, "Загружен набор из {0} изменения.", "Загружен набор из {0} изменений.", "Загружен набор из {0} изменений."));
		}

		public virtual void AddFieldChange(FieldChange change)
		{
			change.ChangeSet = this;
			Changes.Add(change);
		}
	}

	public enum ChangeSetType
	{
		[Display(Name = "Создание")]
		Create,
		[Display(Name = "Изменение")]
		Change,
		[Display(Name = "Удаление")]
		Delete
	}

	public class ChangeSetTypeStringType : NHibernate.Type.EnumStringType
	{
		public ChangeSetTypeStringType() : base(typeof(ChangeSetType))
		{
		}
	}
}

