using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using KellermanSoftware.CompareNetObjects;
using MySql.Data.MySqlClient;
using QSOrmProject.Deletion;
using QSProjectsLib;

namespace QSHistoryLog
{
	public static class HistoryMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public static List<HistoryObjectDesc> ObjectsDesc = new List<HistoryObjectDesc> ();
		const string FieldNameSeparator = ".";

		static CompareLogic qsCompareLogic;

		public static CompareLogic QSCompareLogic {
			get {
				if (qsCompareLogic == null) {
					qsCompareLogic = createQSCompareLogic ();
				}
				return qsCompareLogic;
			}
		}

		static CompareLogic createQSCompareLogic ()
		{
			var logic = new CompareLogic ();
			//Должно стоять в true иначе не работает сравнение на винде Dictionary, так как в KeyValuePair поля со значением canWrite = false
			logic.Config.CompareReadOnly = true; 
			logic.Config.CompareStaticFields = false;
			logic.Config.CompareStaticProperties = false;
			logic.Config.IgnoreCollectionOrder = true;
			logic.Config.MaxDifferences = 10000;
			logic.Config.MaxStructDepth = 10;
			logic.Config.CustomComparers.Add (new DomainObjectComparer(RootComparerFactory.GetRootComparer()));
			logic.Config.AttributesToIgnore.Add (typeof(IgnoreHistoryTraceAttribute));

			return logic;
		}

		public static HistoryObjectDesc AddClass (Type type)
		{
			var desc = new HistoryObjectDesc (type);
			ObjectsDesc.Add (desc);
			return desc;
		}

		/// <summary>
		/// Adds the type for the identifier comparation in collectoions.
		/// </summary>
		/// <param name="type">Type.</param>
		public static void AddIdComparationType(Type type)
		{
			QSCompareLogic.Config.CollectionMatchingSpec.Add (type, new string[] { "Id" });
		}

		public static void AddIdComparationType(Type type, string[] fields)
		{
			QSCompareLogic.Config.CollectionMatchingSpec.Add (type, fields);
		}

		public static string ResolveFieldNameFromPath (string path, bool cutClass = true)
		{
			string result = String.Empty;
			string[] parts = Regex.Split (path, @"\.(.*\[.*\]|.+)(?:\.|$)");

			if (parts.Length <= 0)
				return result;
			var desc = ObjectsDesc.Find (d => d.ObjectName == parts [0]);
			System.Type classType = desc != null ? desc.ObjectType : FineClass (parts [0]);

			if (desc == null && classType != null)
				desc = new HistoryObjectDesc (classType);

			if (!cutClass) {
				if (desc != null)
					result = desc.DisplayName + FieldNameSeparator;
				else
					result = parts [0] + FieldNameSeparator;
			}
				
			if (classType == null)
				for (int i = 1; i < parts.Length; i++)
					result += parts [i] + FieldNameSeparator;
			else if (parts.Length > 1) {
				result += ResolveFieldName (classType, parts.Where ((val, idx) => idx != 0).ToArray ());
			}
					
			return result.TrimEnd (FieldNameSeparator.ToCharArray ());
		}

		private static Type FineClass (string className)
		{
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type t in a.GetTypes()) {
					if (t.Name == className)
						return t;
				}
			}
			return null;
		}

		private static string ResolveFieldName (Type parentClass, string[] path)
		{
			string prop, key = String.Empty;
			var reg = Regex.Match (path [0], @"^(.*)\[(.*)\]$");
			if (reg.Success) {
				prop = reg.Groups [1].Value;
				key = reg.Groups [2].Value;
			} else {
				prop = path [0];
			}

			var field = parentClass.GetProperty (prop);
			if (field == null)
				return String.Join (FieldNameSeparator, path);
				
			var att = field.GetCustomAttributes (typeof(DisplayAttribute), false);
			string name = att.Length > 0 ? (att [0] as DisplayAttribute).GetName () : path [0];

			if(!String.IsNullOrEmpty (key))
			{
				var desc = ObjectsDesc.Find (d => d.ObjectType == parentClass);
				if(desc != null)
				{
					if(desc.PropertiesKeyTitleFunc.ContainsKey (prop))
					{
						var title = desc.PropertiesKeyTitleFunc[prop] (key);
						if (!String.IsNullOrEmpty (title))
							key = title;
					}
				}
				name += String.Format ("[{0}]", key);
			}

			if (path.Length > 1) {
				string recusiveFieldName;
				if(field.PropertyType.IsGenericType && field.PropertyType.GetGenericTypeDefinition () == typeof(List<>))
					recusiveFieldName = ResolveFieldName (field.PropertyType.GetGenericArguments ()[0], path.Where ((val, idx) => idx != 0).ToArray ());
				else
					recusiveFieldName =	ResolveFieldName (field.PropertyType, path.Where ((val, idx) => idx != 0).ToArray ());
				return name + FieldNameSeparator + recusiveFieldName;
			}
			else
				return name + FieldNameSeparator;
		}

		internal static string GetObjectTilte(object value)
		{
			return String.Format ("[{0}]", QSOrmProject.DomainHelper.GetObjectTilte(value));
		}

		internal static int? GetObjectId(object value)
		{
			var prop = value.GetType ().GetProperty ("Id");
			return prop != null ? (int?)prop.GetValue (value, null) : null;
		}

		public static void SubscribeToDeletion()
		{
			DeleteConfig.AfterDeletion += OnOrmAfterDeletion;
		}

		static void OnOrmAfterDeletion (object sender, AfterDeletionEventArgs e)
		{
			logger.Debug ("Записываем ChangeSet-ы удаления в БД.");
			string sql = "INSERT INTO history_changeset (datetime, user_id, operation, object_name, object_id, object_title) " +
				"VALUES ( UTC_TIMESTAMP(), @user_id, @operation, @object_name, @object_id, @object_title)";
			var trans = (MySqlTransaction)e.CurTransaction;

			MySqlCommand cmd = new MySqlCommand(sql, trans.Connection, trans);
			cmd.Prepare ();

			cmd.Parameters.Add("user_id", MySqlDbType.UInt32);
			cmd.Parameters.AddWithValue("operation", ChangeSetType.Delete.ToString ("G"));
			cmd.Parameters.Add("object_name", MySqlDbType.String);
			cmd.Parameters.Add("object_id", MySqlDbType.UInt32);
			cmd.Parameters.Add("object_title", MySqlDbType.String);

			uint count = 0;

			foreach(var item in e.DeletedItems)
			{
				if(!ObjectsDesc.Exists (d => d.ObjectType == item.ItemClass))
				{
					logger.Debug ("Запись в историю информации об удалении объекта, попущена так как не найдено описание класса {0}.", item.ItemClass);
					continue;
				}

				//Обход проблемы удаления пользователем самого себя
				if (item.ItemClass.Name == "User" && item.ItemId == QSMain.User.Id)
					cmd.Parameters ["user_id"].Value = null;
				else
					cmd.Parameters ["user_id"].Value = QSMain.User.Id;

				cmd.Parameters ["object_name"].Value = item.ItemClass.Name;
				cmd.Parameters ["object_id"].Value = item.ItemId;
				cmd.Parameters ["object_title"].Value = item.Title;

				cmd.ExecuteNonQuery();
				count++;
			}
			logger.Debug ("Зафиксировано в журнал удаление {0} объектов.", count);
		}
	}

	public enum ChangeSetType
	{
		[ItemTitle ("Создание")]
		Create,
		[ItemTitle ("Изменение")]
		Change,
		[ItemTitle ("Удаление")]
		Delete
	}

	public enum FieldChangeType
	{
		[ItemTitle ("Добавлено")]
		Added,
		[ItemTitle ("Изменено")]
		Changed,
		[ItemTitle ("Удалено")]
		Removed,
		[ItemTitle ("Без изменений")]
		Unchanged
	}

	public interface IFileTrace
	{
		string Name { set; get; }

		uint Size { set; get; }

		bool IsChanged { set; get; }
	}
}

