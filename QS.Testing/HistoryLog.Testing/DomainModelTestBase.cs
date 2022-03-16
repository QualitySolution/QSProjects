using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.HistoryLog;
using QS.Project.DB;

namespace QS.Testing.HistoryLog.Testing
{
	public abstract class DomainModelTestBase
	{
		
		public static IEnumerable TrackedProperties {
			get {
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(IsTracked)){
					foreach(var prop in mapping.PropertyIterator.Where(IsTracked)) {
						yield return new TestCaseData(mapping, prop)
							.SetArgDisplayNames(new[] { mapping.MappedClass.Name, prop.Name });
					}
				}
			}
		}

		#region Private
		static bool IsTracked(PersistentClass persistentClass)
		{
			return persistentClass.MappedClass.GetCustomAttributes(typeof(HistoryTraceAttribute), true).Length > 0;
		}

		static bool IsTracked(Property property)
		{
			var propInfo = property.PersistentClass.MappedClass.GetProperty(property.Name);
			return propInfo.GetCustomAttributes(typeof(IgnoreHistoryTraceAttribute), true).Length == 0;
		}
		#endregion
		
		/// <summary>
		/// Тест на наличие атрибута с именем свойства для всех отслеживаемых свойств
		/// </summary>
		public virtual void ExistPropertyNameTest(PersistentClass mapping, Property property)
		{
			var propInfo = property.PersistentClass.MappedClass.GetProperty(property.Name);
			var attribute = propInfo.GetCustomAttribute<DisplayAttribute>(true);
			Assert.That(attribute?.Name, Is.Not.Empty.And.Not.Null, "Свойство {0}.{1} отслеживается HistoryLog, при этом не имеет атрибута [Display(Name = ...)].",
				mapping.MappedClass,
				property.Name);
		}
	}
}