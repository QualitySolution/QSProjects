using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.Project.DB;

namespace QS.Deletion.Testing
{
	public abstract class DeleteConfigTestBase
	{
		#region Проверка зависимостей в настроенных кассах удаления

		public static IEnumerable AllDeleteItems {
			get {
				Console.WriteLine("AllDeleteItems");
				foreach(var info in DeleteConfig.ClassDeleteRules) {
					foreach(var item in info.DeleteItems) {
						yield return new TestCaseData(info, item)
						.SetArgDisplayNames(new [] { item.ObjectClass.Name, info.ObjectClass.Name});
					}
				}
			}
		}

		public virtual void DeleteItemsTypesTest(IDeleteRule info, DeleteDependenceInfo dependence)
		{
			if(!String.IsNullOrEmpty(dependence.PropertyName)) {
				Assert.That(info.ObjectClass, Is.EqualTo(dependence.ObjectClass.GetProperty(dependence.PropertyName).PropertyType),
					"#Свойство {0}.{1} определенное в зависимости удаления для класса {2}, имеет другой тип.",
					dependence.ObjectClass.Name,
					dependence.PropertyName,
					info.ObjectClass
				);
				Assert.Pass();
			}

			Assert.Ignore("Для этой зависимости удаления нет необходимости тестировать типы.");
		}

		public static IEnumerable AllClearItems {
			get {
				Console.WriteLine("AllClearItems");
				foreach(var info in DeleteConfig.ClassDeleteRules) {
					foreach(var item in info.ClearItems) {
						yield return new TestCaseData(info, item);
					}
				}
			}
		}

		public virtual void ClearItemsTypesTest(IDeleteRule info, ClearDependenceInfo dependence)
		{
			if(!String.IsNullOrEmpty(dependence.PropertyName)) {
				Assert.That(info.ObjectClass, Is.EqualTo(dependence.ObjectClass.GetProperty(dependence.PropertyName).PropertyType),
					"##Свойство {0}.{1} определенное в зависимости очистки для класса {2}, имеет другой тип.",
					dependence.ObjectClass.Name,
					dependence.PropertyName,
					info.ObjectClass
				);
				Assert.Pass();
			}

			Assert.Ignore("Для этой зависимости очистки нет необходимости тестировать типы.");
		}

		#endregion
		#region Проверка наличия правил удаления.

		public static IEnumerable NhibernateMappedClasses {
			get {
				Console.WriteLine("NhibernateMappedClasses");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings) {
					yield return new TestCaseData(mapping);
				}
			}
		}

		public static List<Type> IgnoreMissingClass = new List<Type>();

		/// <summary>
		/// Тест наличия правил удаления для классов добавленных в Nhibernate.
		/// Чтобы исключить класс из проверки добавьте его в коллекцию IgnoreMissingClass
		/// </summary>
		public virtual void DeleteRuleExisitForNHMappedClasssTest(NHibernate.Mapping.PersistentClass mapping)
		{
			if(IgnoreMissingClass.Contains(mapping.MappedClass)) {
				Assert.Ignore($"Класс {mapping.MappedClass} добавлен в исключения");
			}

			var exist = DeleteConfig.ClassDeleteRules.Any(i => i.ObjectClass == mapping.MappedClass);

			Assert.That(exist, "#Класс {0} настроен в мапинге NHibernate, но для него отсутствуют правила удаления.", mapping.MappedClass);
		}

		#endregion

		#region Проверка обычных ссылко в Nhibernate на наличие правил удаления

		public static IEnumerable NhibernateMappedEntityRelation {
			get {
				Console.WriteLine("NhibernateMappedEntityRelation");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(m => !IgnoreMissingClass.Contains(m.MappedClass))){
					foreach(var prop in mapping.PropertyIterator.Where(p => p.IsEntityRelation)) {
						yield return new TestCaseData(mapping, prop);
					}
				}
			}
		}

		/// <summary>
		/// Тест наличия правил удаления для ссылок на другие сущьности в класе.
		/// Чтобы исключить класс из проверки добавьте его в коллекцию IgnoreMissingClass
		/// </summary>
		public virtual void DeleteRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property prop)
		{
			var propType = prop.Type.ReturnedClass;
			var exist = DeleteConfig.ClassDeleteRules.Any(c => c.ObjectClass == propType);
			Assert.That(exist, "#Класс {0} не имеет правил удаления, но свойство {1}.{2} прописано в мапинге.", 
					prop.Type.ReturnedClass.Name,
					mapping.MappedClass,
					prop.Name);
		}

		public static IEnumerable NhibernateMappedEntityRelationWithExistRule {
			get {
				Console.WriteLine("NhibernateMappedEntityRelationWithExistRule");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(m => !IgnoreMissingClass.Contains(m.MappedClass))) {
					foreach(var prop in mapping.PropertyIterator.Where(p => p.IsEntityRelation)) {
						var related = DeleteConfig.ClassDeleteRules.FirstOrDefault(c => c.ObjectClass == prop.Type.ReturnedClass);
						if(related == null)
							continue;
						yield return new TestCaseData(mapping, prop, related);
					}
				}
			}
		}

		/// <summary>
		/// Тест наличия зависимости для ссылок на другие сущьности в класе.
		/// </summary>
		public virtual void DependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property prop, IDeleteRule related)
		{
			Assert.That(related.DeleteItems.Exists(r => r.ObjectClass == mapping.MappedClass && r.PropertyName == prop.Name)
						|| related.ClearItems.Exists(r => r.ObjectClass == mapping.MappedClass && r.PropertyName == prop.Name),
				"#Для свойства {0}.{1} не определены зависимости удаления в классе {2}",
					mapping.MappedClass,
					prop.Name,
					related.ObjectClass.Name);
		}

		public static IEnumerable NhibernateMappedEntityRelationWithExistRuleCascadeRelated {
			get {
				Console.WriteLine("NhibernateMappedEntityRelationWithExistRuleCascadeRelated");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(m => !IgnoreMissingClass.Contains(m.MappedClass))) {
					foreach(var prop in mapping.PropertyIterator.Where(p => p.IsEntityRelation)) {
						var related = DeleteConfig.ClassDeleteRules.OfType<IHibernateDeleteRule>().FirstOrDefault(c => c.ObjectClass == prop.Type.ReturnedClass);
						if(related == null || !related.IsRequiredCascadeDeletion )
							continue;
						yield return new TestCaseData(mapping, prop, related);
					}
				}
			}
		}

		/// <summary>
		/// Тест наличия каскадной зависимости для ссылок на другие сущьности в класе.
		/// </summary>
		public virtual void CascadeDependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property prop, IDeleteRule related)
		{
			var info = DeleteConfig.GetDeleteRule(mapping.MappedClass);
			Assert.That(info.DeleteItems.Any(x => x.ParentPropertyName == prop.Name && x.IsCascade),
					"#Cвойство {0}.{1} не имеет каскадного правила удаления, хотя класс {2} помечен как требующий каскадного удаления.",
					info.ObjectClass.Name,
					prop.Name,
					related.ObjectClass.Name);
		}

		#endregion
	}
}
