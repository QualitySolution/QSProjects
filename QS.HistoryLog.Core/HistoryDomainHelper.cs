using QS.DomainModel.Entity;
using QS.HistoryLog.Core.Attributes;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.HistoryLog.Core {
	public class HistoryDomainHelper {

		internal static string GetObjectTitle(object value) {
			return value == null ? null : String.Format("[{0}]", DomainHelper.GetTitle(value));
		}

		public static string ResolveFieldTitle(string clazz, string fieldName) {
			var type = FineEntityClass(clazz);
			
			if(type != null) {
				return DomainHelper.GetPropertyTitle(type, fieldName) ?? fieldName;
			}
			return fieldName;
		}

		public static IEnumerable<HistoryObjectDesc> TraceClasses {
			get {
				return OrmConfig.NhConfig.ClassMappings
					.Where(x => x.MappedClass.GetCustomAttributes(typeof(HistoryTraceAttribute), true).Length > 0)
					.Select(x => new HistoryObjectDesc(x.MappedClass));
			}
		}

		internal static Type FineEntityClass(string className) {
			return OrmConfig.NhConfig.ClassMappings
					.Where(x => x.MappedClass.Name == className)
					.Select(x => x.MappedClass)
					.FirstOrDefault();
		}
	}
}
