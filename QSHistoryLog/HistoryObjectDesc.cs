using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;
using QS.Utilities.Text;
using QSOrmProject;

namespace QSHistoryLog
{
	public class HistoryObjectDesc
	{
		public string ObjectName { get; set;}
		public Type ObjectType { get; set;}
		public string DisplayName { get; set;}

		public IEnumerable<HistoryFieldDesc> TracedProperties{
			get {
				var persistent = OrmMain.OrmConfig.GetClassMapping(ObjectType);

				foreach(var propertyMap in persistent.PropertyIterator)
				{
					var propInfo = persistent.MappedClass.GetProperty(propertyMap.Name);
					if(propInfo.GetCustomAttributes(typeof(IgnoreHistoryTraceAttribute), true).Length > 0)
						continue;

					var att = propInfo.GetCustomAttributes (typeof(DisplayAttribute), false);
					if(att.Length > 0)
					{
						yield return new HistoryFieldDesc {
							FieldName = propertyMap.Name,
							DisplayName = (att [0] as DisplayAttribute).GetName ()
						};
					}
				}
			}
		}

		public HistoryObjectDesc(Type type)
		{
			ObjectType = type;
			ObjectName = type.Name;

			var att = type.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0 && !String.IsNullOrWhiteSpace((att [0] as OrmSubjectAttribute).ObjectName))
				DisplayName = (att[0] as OrmSubjectAttribute).ObjectName.StringToTitleCase();
			else
				DisplayName = ObjectName;
		}
	}

	public class HistoryFieldDesc
	{
		public string FieldName { get; set;}
		public string DisplayName { get; set;}
	}
}
