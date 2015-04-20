using System;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using KellermanSoftware.CompareNetObjects;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace QSHistoryLog
{
	public class ObjectTracker<T> where T : class
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		object firstObject, lastObject;
		ComparisonResult compare;

		private string objectName;
		public int ObjectId;
		private string objectTitle;

		private long changeSetId;

		public ChangeSetType operation = ChangeSetType.Create;

		public bool HasChanges {
			get {
				if (compare != null)
					return !compare.AreEqual;
				else
					return false;
			}
		}

		public ObjectTracker ()
		{
		}

		public ObjectTracker (T subject)
		{
			TakeEmpty (subject);
		}

		public void TakeFirst(T subject)
		{
			lastObject = null;
			compare = null;
			operation = ChangeSetType.Change;
			firstObject = NClone.Clone.ObjectGraph (subject);
		}

		public void TakeEmpty(T subject)
		{
			lastObject = null;
			compare = null;
			operation = ChangeSetType.Create;
			firstObject = NClone.Clone.ObjectGraph (subject);
		}

		public void TakeLast(T subject)
		{
			compare = null;
			lastObject = NClone.Clone.ObjectGraph (subject);
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

			prop = typeof(T).GetProperty ("Name");
			if (String.IsNullOrEmpty (objectTitle) && prop != null)
				objectTitle = (string)prop.GetValue (subject, null);
		}

		/// <summary>
		/// Возвращает true если объекты различаются.
		/// </summary>
		public bool Compare()
		{
			if (firstObject == null)
				throw new InvalidOperationException ("Перед сравнением необходимо сделать первый снимок c помощью TakeFirst или TakeEmpty.");

			if (lastObject == null)
				throw new InvalidOperationException ("Перед сравнением необходимо сделать последний снимок c помощью TakeLast");

			compare = HistoryMain.QSCompareLogic.Compare (firstObject, lastObject);

			logger.Debug (compare.DifferencesString);

			return !compare.AreEqual;
		}

		public void SaveChangeSet(MySqlTransaction trans)
		{
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
			sql = "INSERT INTO history_changes (changeset_id, path, type, old_id, old_value, new_id, new_value) " +
				"VALUES (@changeset_id, @path, @type, @old_id, @old_value, @new_id, @new_value)";

			cmd = new MySqlCommand(sql, trans.Connection, trans);
			cmd.Prepare ();
			cmd.Parameters.AddWithValue ("changeset_id", changeSetId);
			cmd.Parameters.Add ("path", MySqlDbType.String);
			cmd.Parameters.Add ("type", MySqlDbType.Enum);
			cmd.Parameters.Add ("old_value", MySqlDbType.Text);
			cmd.Parameters.Add ("new_value", MySqlDbType.Text);
			cmd.Parameters.Add ("old_id", MySqlDbType.UInt32);
			cmd.Parameters.Add ("new_id", MySqlDbType.UInt32);
				
			foreach(var onechange in compare.Differences)
			{
				if (!FixDisplay (onechange))
					continue;
				string modifedPropName = Regex.Replace (onechange.PropertyName, @"(^.*)\[Key:(.*)\]\.Value$", m => String.Format ("{0}[{1}]", m.Groups [1].Value, m.Groups [2].Value));
				if (onechange.ParentObject2 != null && onechange.ParentObject2.Target != null) {
					var id = HistoryMain.GetObjectId (onechange.ParentObject2.Target);
					if(id.HasValue)
						modifedPropName = Regex.Replace (modifedPropName, String.Format (@"\[Id:{0}\]", id.Value), HistoryMain.GetObjectTilte (onechange.ParentObject2.Target));//FIXME Тут неочевидно появляются квадратные скобки
				}
				cmd.Parameters ["path"].Value = objectName + modifedPropName;
				if(onechange.Object1 == null || onechange.Object1.Target == null)
					cmd.Parameters ["type"].Value = FieldChangeType.Added;
				else if(onechange.Object2 == null || onechange.Object2.Target == null)
					cmd.Parameters ["type"].Value = FieldChangeType.Removed;
				else
					cmd.Parameters ["type"].Value = FieldChangeType.Changed;

				cmd.Parameters ["old_value"].Value = onechange.Object1Value;
				cmd.Parameters ["new_value"].Value = onechange.Object2Value;
				if (onechange.ChildPropertyName == "Id") {
					cmd.Parameters ["old_id"].Value = DBWorks.IdPropertyOrNull (onechange.Object1.Target);
					cmd.Parameters ["new_id"].Value = DBWorks.IdPropertyOrNull (onechange.Object2.Target);
				} else
					cmd.Parameters ["old_id"].Value = cmd.Parameters ["new_id"].Value = DBNull.Value;

				cmd.ExecuteNonQuery ();
			}
			logger.Debug ("Зафиксированы изменения в {0} полях.", compare.Differences.Count);
		}
			
		/// <returns><c>false</c> если нужно не сохранять элемент.</returns>
		/// <param name="diff">Diff.</param>
		private bool FixDisplay(Difference diff)
		{
			//DateTime
			if (diff.Object1 != null && diff.Object1.Target is DateTime) {
				if ((DateTime)diff.Object1.Target == default(DateTime)) {
					diff.Object1.Target = null;
					diff.Object1Value = String.Empty;
				} else if (((DateTime)diff.Object1.Target).TimeOfDay.Ticks == 0)
					diff.Object1Value = ((DateTime)diff.Object1.Target).ToShortDateString ();

				if ((DateTime)diff.Object2.Target == default(DateTime)) {
					diff.Object2.Target = null;
					diff.Object2Value = String.Empty;
				} else if (((DateTime)diff.Object2.Target).TimeOfDay.Ticks == 0)
					diff.Object2Value = ((DateTime)diff.Object2.Target).ToShortDateString ();
			}

			//Добавление и удаление объектов в коллекциях
			if(diff.ParentObject2 != null && diff.ParentObject2.Target != null 
			   && diff.ParentObject2.Target.GetType ().IsGenericType 
			   && diff.ParentObject2.Target.GetType ().GetGenericTypeDefinition () == typeof(List<>) 
			   && diff.Object1 == null)
			{
				diff.PropertyName = Regex.Replace (diff.PropertyName, @"\[Id:.*\]$", "[+]");
				diff.Object1Value = String.Empty;
				diff.Object2Value = HistoryMain.GetObjectTilte (diff.Object2.Target);
			}
			if(diff.ParentObject1 != null && diff.ParentObject1.Target != null 
			   && diff.ParentObject1.Target.GetType ().IsGenericType 
			   && diff.ParentObject1.Target.GetType ().GetGenericTypeDefinition () == typeof(List<>) 
			   && diff.Object2 == null)
			{
				diff.PropertyName = Regex.Replace (diff.PropertyName, @"\[Id:.*\]$", "[-]");
				diff.Object2Value = String.Empty;
				diff.Object1Value = HistoryMain.GetObjectTilte (diff.Object1.Target);
			}

			//IFileTrace
			if(diff.ParentObject1 != null && diff.ParentObject1.Target is IFileTrace)
			{
				if (Regex.IsMatch (diff.PropertyName, @".*Size$"))
					return false;
				if(Regex.IsMatch (diff.PropertyName, @".*IsChanged$"))
				{   
					diff.PropertyName = diff.PropertyName.Replace ("IsChanged", "Size");
					diff.Object1Value = diff.ParentObject1 != null && diff.ParentObject1.Target != null 
						? StringWorks.BytesToIECUnitsString ((diff.ParentObject1.Target as IFileTrace).Size) : String.Empty;
					diff.Object2Value = diff.ParentObject2 != null && diff.ParentObject2.Target != null 
						? StringWorks.BytesToIECUnitsString ((diff.ParentObject2.Target as IFileTrace).Size) : String.Empty;
				}
			}

			return true;
		}
	}
}

