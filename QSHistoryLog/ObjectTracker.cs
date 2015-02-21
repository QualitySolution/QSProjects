using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SIT.Components.ObjectComparer;
using QSProjectsLib;

namespace QSHistoryLog
{
	public class ObjectTracker<T>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		Snapshot firstSnapshot;
		Snapshot lastSnapshot;
		CompareItem compare;
		ChangeSet changeset;
		List<ChangeSet> listOfChanges;

		private string objectName;
		public int ObjectId;
		private string objectTitle;

		private long changeSetId;

		public ChangeSetType operation = ChangeSetType.create;
		public bool HasChanges;

		public ObjectTracker ()
		{
		}

		public ObjectTracker (T subject)
		{
			TakeEmpty (subject);
		}

		public void TakeFirst(T subject)
		{
			lastSnapshot = null;
			operation = ChangeSetType.change;
			firstSnapshot = new ObjectSnapshot (subject);
		}

		public void TakeEmpty(T subject)
		{
			lastSnapshot = null;
			operation = ChangeSetType.create;
			firstSnapshot = new ObjectSnapshot (subject);
		}

		public void TakeLast(T subject)
		{
			lastSnapshot = new ObjectSnapshot (subject);
			ReadObjectDiscription (subject);
		}

		private void ReadObjectDiscription(T subject)
		{
			objectName = subject.GetType ().Name;
			var prop = typeof(T).GetProperty ("Id");
			if (prop != null)
				ObjectId = (int)prop.GetValue (subject, null);

			prop = typeof(T).GetProperty ("Title");
			if (prop != null)
				objectTitle = (string)prop.GetValue (subject, null);
		}

		public bool Compare()
		{
			if (firstSnapshot == null)
				throw new InvalidOperationException ("Перед сравнением необходимо сделать первый снимок c помощью TakeFirst или TakeEmpty.");

			if (lastSnapshot == null)
				throw new InvalidOperationException ("Перед сравнением необходимо сделать последний снимок c помощью TakeLast");

			compare = new ObjectCompareItem();
			compare.Create (firstSnapshot, lastSnapshot);
			changeset = new ChangeSet ();
			changeset.Create (compare);

			listOfChanges = changeset.Flatten ();
			HasChanges = listOfChanges.Count > 0;
			return HasChanges;
		}

		public void SaveChangeSet(MySqlTransaction trans)
		{
			if (listOfChanges == null)
				Compare ();
			if (!HasChanges) {
				logger.Warn ("Нет изменнений. Нечего записывать.");
				return;
			}
			if (ObjectId <= 0)
				throw new InvalidOperationException ("Перед записью changeset-а для нового объекта после записи его в БД необходимо вручную прописать его Id в поле ObjectId.");

			logger.Debug ("Записываем ChangeSet в БД.");
			string sql = "INSERT INTO history_changeset (datetime, user_id, operation, object_name, object_id, object_title) " +
				"VALUES ( UTC_TIMESTAMP(), @user_id, @operation, @object_name, @object_id, @object_title)";

			MySqlCommand cmd = new MySqlCommand(sql, trans.Connection, trans);

			cmd.Parameters.AddWithValue("@user_id", QSMain.User.id);
			cmd.Parameters.AddWithValue("@operation", operation.ToString("G"));
			cmd.Parameters.AddWithValue("@object_name", objectName);
			cmd.Parameters.AddWithValue("@object_id", ObjectId);
			cmd.Parameters.AddWithValue("@object_title", DBWorks.ValueOrNull (objectTitle != "", objectTitle));

			cmd.ExecuteNonQuery();
			changeSetId = cmd.LastInsertedId;

			logger.Debug ("Записываем изменения полей в ChangeSet-е.");
			sql = "INSERT INTO history_changes (changeset_id, field, old_value, new_value) " +
				"VALUES (@changeset_id, @field, @old_value, @new_value)";

			cmd = new MySqlCommand(sql, trans.Connection, trans);
			cmd.Prepare ();
			cmd.Parameters.AddWithValue ("changeset_id", changeSetId);
			cmd.Parameters.Add ("field", MySqlDbType.String);
			cmd.Parameters.Add ("old_value", MySqlDbType.Text);
			cmd.Parameters.Add ("new_value", MySqlDbType.Text);
				
			foreach(ChangeSet onechange in listOfChanges)
			{
				cmd.Parameters ["field"].Value = onechange.Path;
				cmd.Parameters ["old_value"].Value = onechange.ValueA;
				cmd.Parameters ["new_value"].Value = onechange.ValueB;
				cmd.ExecuteNonQuery ();
			}
			logger.Debug ("Зафиксированы изменения в {0} полях.", listOfChanges.Count);
		}
	}
		
}

