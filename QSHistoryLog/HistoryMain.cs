using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System.Data.Bindings;

namespace QSHistoryLog
{
	public static class HistoryMain
	{
		public static List<HistoryObjectDesc> ObjectsDesc = new List<HistoryObjectDesc>();

		public static void AddClass(Type type)
		{
			ObjectsDesc.Add (new HistoryObjectDesc (type));
		}
	}

	public enum ChangeSetType{
		[ItemTitle("Создание")]
		Create,
		[ItemTitle("Изменение")]
		Change,
		[ItemTitle("Удаление")]
		Delete
	}

	public class HistoryObjectDesc
	{
		public string ObjectName { get; set;}
		public Type ObjectType { get; set;}
		public string DisplayName { get; set;}

		public HistoryObjectDesc(Type type)
		{
			ObjectType = type;
			ObjectName = type.Name;

			var att = type.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0 && !String.IsNullOrWhiteSpace((att [0] as OrmSubjectAttribute).Name))
				DisplayName = (att [0] as OrmSubjectAttribute).Name;
			else
				DisplayName = ObjectName;
		}
	}
}

