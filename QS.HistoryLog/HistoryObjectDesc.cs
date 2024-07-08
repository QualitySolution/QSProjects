using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Project.DB;
using QS.Utilities.Extensions;
using QS.Utilities.Text;

namespace QS.HistoryLog
{
	public class HistoryObjectDesc
	{
		public string ObjectName { get; set;}
		public Type ObjectType { get; set;}
		public string DisplayName { get; set;}

		public IEnumerable<HistoryFieldDesc> TracedProperties{
			get {
				var persistent = OrmConfig.NhConfig.GetClassMapping(ObjectType);

				foreach(var propertyMap in persistent.PropertyIterator)
				{
					var propInfo = persistent.MappedClass.GetPropertyInfo(propertyMap.Name);
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

			var att = type.GetCustomAttributes (typeof(AppellativeAttribute), true);
			if (att.Length > 0 && !String.IsNullOrWhiteSpace((att [0] as AppellativeAttribute).Nominative))
				DisplayName = (att[0] as AppellativeAttribute).Nominative.StringToTitleCase();
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
