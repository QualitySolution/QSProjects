using System;
using System.Collections.Generic;

namespace QSOrmProject
{
	public static class DeleteConfig{
		private static List<DeleteInfo> classInfos;

		internal static List<DeleteInfo> ClassInfos {
			get {if(classInfos == null)
				{
					classInfos = new List<DeleteInfo> ();
					QSProjectsLib.QSMain.RunOrmDeletion += RunDeletionFromProjectLib;
				}
				return classInfos;
			}
		}

		/// <summary>
		/// Необходимо для интеграции с библиотекой QSProjectsLib
		/// </summary>
		static void RunDeletionFromProjectLib (object sender, QSProjectsLib.QSMain.RunOrmDeletionEventArgs e)
		{
			var deleteWin = new DeleteDlg ();
			e.Result = deleteWin.RunDeletion (e.TableName, e.ObjectId);
		}

		public static void AddDeleteInfo(DeleteInfo info)
		{
			if (ClassInfos.Exists (i => i.TableName == info.TableName && i.ObjectClass == info.ObjectClass))
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} и таблицы {1}, уже существует.", info.ObjectClass, info.TableName));

			ClassInfos.Add (info);
		}
				
	}

	public class DeleteInfo
	{
		public Type ObjectClass;
		public string ObjectsName;
		public string ObjectName;
		public string TableName;
		public string SqlSelect;
		public string DisplayString;
		public List<DeleteDependenceInfo> DeleteItems;
		public List<ClearDependenceInfo> ClearItems;

		public DeleteInfo()
		{
			DeleteItems = new List<DeleteDependenceInfo>();
			ClearItems = new List<ClearDependenceInfo>();
		}
	
	}

	public class DeleteDependenceInfo
	{
		public Type ObjectClass;
		public string TableName;

		/// <summary>
		/// В выражении можно использовать параметр @id для получения id удаляемого объекта.
		/// </summary>
		public string WhereStatment;

		public DeleteDependenceInfo(Type objectClass, string sqlwhere)
		{
			ObjectClass = objectClass;
			WhereStatment = sqlwhere;
		}

		public DeleteDependenceInfo(string tableName, string sqlwhere)
		{
			TableName = tableName;
			WhereStatment = sqlwhere;
		}

		public DeleteInfo GetClassInfo()
		{
			if(ObjectClass != null)
				return DeleteConfig.ClassInfos.Find (i => i.ObjectClass == ObjectClass);
			else
				return DeleteConfig.ClassInfos.Find (i => i.TableName == TableName);
		}
	}

	public class ClearDependenceInfo
	{
		public Type ObjectClass;
		public string TableName;
		public string[] ClearFields;

		/// <summary>
		/// В выражении можно использовать параметр @id для получения id удаляемого объекта.
		/// </summary>
		public string WhereStatment;

		public ClearDependenceInfo(Type objectClass, string sqlwhere, params string[] clearField)
		{
			ObjectClass = objectClass;
			WhereStatment = sqlwhere;
			ClearFields = clearField;
		}

		public ClearDependenceInfo(string tableName, string sqlwhere, params string[] clearField)
		{
			TableName = tableName;
			WhereStatment = sqlwhere;
			ClearFields = clearField;
		}

		public DeleteInfo GetClassInfo()
		{
			if(ObjectClass != null)
				return DeleteConfig.ClassInfos.Find (i => i.ObjectClass == ObjectClass);
			else
				return DeleteConfig.ClassInfos.Find (i => i.TableName == TableName);
		}
	}

}

