using System;
using Gtk;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace QSProjectsLib
{
	public class QSMain
	{
		public static MySqlConnection connectionDB;
		public static string ConnectionString;
		public static Dictionary<string, UserPermission> ProjectPermission;
		public static Dictionary<string, TableInfo> ProjectTables;
		public static string AdminFieldName;
		public static UserInfo User;
		//События
		public static event EventHandler<NewStatusTextEventArgs> NewStatusText;
		public static event EventHandler<ReferenceUpdatedEventArgs> ReferenceUpdated;

		public QSMain ()
		{
		}

		//Событие строки состояния
		public class NewStatusTextEventArgs : EventArgs
		{
			public string NewText { get; set; }
		}

		internal static void OnNewStatusText(string NewText)
		{
			EventHandler<NewStatusTextEventArgs> handler = NewStatusText;
			if (handler != null)
			{
				NewStatusTextEventArgs e = new NewStatusTextEventArgs();
				e.NewText = NewText;
				handler(null, e);
			}
		}

		//Событие обновления справочников
		public class ReferenceUpdatedEventArgs : EventArgs
		{
			public string ReferenceTable { get; set; }
		}
		
		internal static void OnReferenceUpdated(string Table)
		{
			EventHandler<ReferenceUpdatedEventArgs> handler = ReferenceUpdated;
			if (handler != null)
			{
				ReferenceUpdatedEventArgs e = new ReferenceUpdatedEventArgs();
				e.ReferenceTable = Table;
				handler(null, e);
			}
		}

		internal static bool TestConnection()
		{
			if(connectionDB == null)
			{
				QSMain.OnNewStatusText("Не передано соединение с БД!");
				return false;
			}
			return true;
		}

		public static string GetPermissionFieldsForSelect()
		{
			if(ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach( KeyValuePair<string, UserPermission> Right in ProjectPermission)
			{
				FieldsString += ", " + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static string GetPermissionFieldsForInsert()
		{
			if(ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach( KeyValuePair<string, UserPermission> Right in ProjectPermission)
			{
				FieldsString += ", @" + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static string GetPermissionFieldsForUpdate()
		{
			if(ProjectPermission == null)
				return "";
			string FieldsString = "";
			foreach( KeyValuePair<string, UserPermission> Right in ProjectPermission)
			{
				FieldsString += ", " + Right.Value.DataBaseName + " = @" + Right.Value.DataBaseName;
			}
			return FieldsString;
		}

		public static void ErrorMessage(Window parent, Exception ex)
		{
			MessageDialog md = new MessageDialog ( parent, DialogFlags.DestroyWithParent,
			                                      MessageType.Error, 
			                                      ButtonsType.Close,"ошибка");
			md.UseMarkup = false;
			md.Text = ex.ToString();
			md.Run ();
			md.Destroy();
		}
	}
}

