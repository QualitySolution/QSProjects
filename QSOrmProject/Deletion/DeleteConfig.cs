using System;
using System.Collections.Generic;

namespace QSOrmProject.Deletion
{
	public static partial class DeleteConfig{
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

		public static event EventHandler<AfterDeletionEventArgs> AfterDeletion;

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

		public static void AddDeleteDependence<ToClass>(DeleteDependenceInfo deleteDependence)
		{
			var info = ClassInfos.Find (i => i.ObjectClass == typeof(ToClass));

			if(info == null)
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} не найдено.", typeof(ToClass)));

			info.DeleteItems.Add(deleteDependence);
		}

		public static void AddClearDependence<ToClass>(ClearDependenceInfo clearDependence)
		{
			var info = ClassInfos.Find (i => i.ObjectClass == typeof(ToClass));

			if(info == null)
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} не найдено.", typeof(ToClass)));

			info.ClearItems.Add(clearDependence);
		}
			
		internal static void OnAfterDeletion(System.Data.Common.DbTransaction trans, List<DeletedItem> items)
		{
			if(AfterDeletion != null)
			{
				AfterDeletion (null, new AfterDeletionEventArgs {
					CurTransaction = trans,
					DeletedItems = items
				});
			}
		}
	}

	public class AfterDeletionEventArgs : EventArgs
	{
		public System.Data.Common.DbTransaction CurTransaction;
		public List<DeletedItem> DeletedItems;
	}
}
