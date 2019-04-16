using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.Project.DB;

namespace QS.Deletion.Testing
{
	public abstract class DeleteConfigTestBase
	{
		#region Настройка поведения тестов

		#region Игнорирование классов

		/// <summary>
		/// Игнорируем в тестах отсутствие правил удаления для класов.
		/// </summary>
		internal static Dictionary<Type, string> IgnoreMissingClass = new Dictionary<Type, string>();

		/// <summary>
		/// Добавляем класса в игнорируемые в тестах отсутствие правил.
		/// </summary>
		public static void AddIgnoredClass(Type type, string reason)
		{
			IgnoreMissingClass[type] = reason;
		}

		public static void AddIgnoredClass<T>(string reason)
		{
			AddIgnoredClass(typeof(T), reason);
		}

		#endregion
		#region Игнорирование свойств

		/// <summary>
		/// Игнорируем в тестах отсутствие зависимостей для перечисленных свойств каласов
		/// </summary>
		/// <remarks>Внимание, игнорирование реализовано не для всех тестов!</remarks>
		public static Dictionary<Type, List<IgnoredProperty>> IgnoreProperties = new Dictionary<Type, List<IgnoredProperty>>();

		/// <summary>
		/// Добавляем свойство класса игнорируемое в тестах отсутствие зависимостей
		/// </summary>
		/// <remarks>Внимание, игнорирование реализовано не для всех тестов!</remarks>
		public static void AddIgnoredProperty(Type type, string name, string reason)
		{
			var prop = new IgnoredProperty {
				PropertyName = name,
				ReasonForIgnoring = reason
			};

			if(!IgnoreProperties.ContainsKey(type))
				IgnoreProperties.Add(type, new List<IgnoredProperty>());

			IgnoreProperties[type].Add(prop);
		}

		public static void AddIgnoredProperty<T>(Expression<Func<T, object>> propertyRefExpr, string reason)
		{
			var name = PropertyUtil.GetName<T>(propertyRefExpr);
			AddIgnoredProperty(typeof(T), name, reason);
		}

		public struct IgnoredProperty
		{
			public string PropertyName;
			public string ReasonForIgnoring;
		}
		#endregion

		#endregion

		#region Проверка зависимостей в настроенных кассах удаления

		public static IEnumerable AllDeleteItems {
			get {
				Console.WriteLine("AllDeleteItems");
				foreach(var info in DeleteConfig.ClassDeleteRules) {
					foreach(var item in info.DeleteItems) {
						yield return new TestCaseData(info, item)
							.SetDescription("Проверка типов зависимостей удаления")
							.SetArgDisplayNames(new [] { info.ObjectClass.Name, item.ObjectClass.Name });
					}
				}
			}
		}

		public virtual void DeleteItemsTypesTest(IDeleteRule info, DeleteDependenceInfo dependence)
		{
			if(!String.IsNullOrEmpty(dependence.PropertyName)) {
				Assert.That(info.ObjectClass, Is.EqualTo(dependence.ObjectClass.GetProperty(dependence.PropertyName).PropertyType),
					"Свойство {0}.{1} определенное в зависимости удаления для класса {2}, имеет другой тип.",
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
						yield return new TestCaseData(info, item)
						.SetDescription("Проверка типов зависимостей очистки")
						.SetArgDisplayNames(new[] { info.ObjectClass.Name, item.ObjectClass.Name });
					}
				}
			}
		}

		public virtual void ClearItemsTypesTest(IDeleteRule info, ClearDependenceInfo dependence)
		{
			if(!String.IsNullOrEmpty(dependence.PropertyName)) {
				Assert.That(info.ObjectClass, Is.EqualTo(dependence.ObjectClass.GetProperty(dependence.PropertyName).PropertyType),
					"Свойство {0}.{1} определенное в зависимости очистки для класса {2}, имеет другой тип.",
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
					yield return new TestCaseData(mapping)
						.SetArgDisplayNames(new[] { mapping.MappedClass.Name });
				}
			}
		}

		/// <summary>
		/// Тест наличия правил удаления для классов добавленных в Nhibernate.
		/// Чтобы исключить класс из проверки добавьте его в коллекцию IgnoreMissingClass
		/// </summary>
		public virtual void DeleteRuleExisitForNHMappedClasssTest(NHibernate.Mapping.PersistentClass mapping)
		{
			if(IgnoreMissingClass.ContainsKey(mapping.MappedClass)) {
				Assert.Ignore(IgnoreMissingClass[mapping.MappedClass]);
			}

			var exist = DeleteConfig.ClassDeleteRules.Any(i => i.ObjectClass == mapping.MappedClass);

			Assert.That(exist, "Класс {0} настроен в мапинге NHibernate, но для него отсутствуют правила удаления.", mapping.MappedClass);
		}

		#endregion

		#region Проверка обычных ссылок в Nhibernate на наличие правил удаления

		public static IEnumerable NhibernateMappedEntityRelation {
			get {
				Console.WriteLine("NhibernateMappedEntityRelation");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(m => !IgnoreMissingClass.ContainsKey(m.MappedClass))){
					foreach(var prop in mapping.PropertyIterator.Where(p => p.IsEntityRelation)) {
						yield return new TestCaseData(mapping, prop)
							.SetArgDisplayNames(new[] { mapping.MappedClass.Name, prop.Name });
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
			if(IgnoreProperties.ContainsKey(mapping.MappedClass) && IgnoreProperties[mapping.MappedClass].Any(x => x.PropertyName == prop.Name))
				Assert.Ignore(IgnoreProperties[mapping.MappedClass].First(x => x.PropertyName == prop.Name).ReasonForIgnoring);

			var propType = prop.Type.ReturnedClass;
			var exist = DeleteConfig.ClassDeleteRules.Any(c => c.ObjectClass == propType);
			Assert.That(exist, "Класс {0} не имеет правил удаления, но свойство {1}.{2} прописано в мапинге.", 
					prop.Type.ReturnedClass.Name,
					mapping.MappedClass,
					prop.Name);
		}

		public static IEnumerable NhibernateMappedEntityRelationWithExistRule {
			get {
				Console.WriteLine("NhibernateMappedEntityRelationWithExistRule");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(m => !IgnoreMissingClass.ContainsKey(m.MappedClass))) {
					foreach(var prop in mapping.PropertyIterator.Where(p => p.IsEntityRelation)) {
						var related = DeleteConfig.ClassDeleteRules.FirstOrDefault(c => c.ObjectClass == prop.Type.ReturnedClass);
						if(related == null)
							continue;
						yield return new TestCaseData(mapping, prop, related)
							.SetArgDisplayNames(new[] { mapping.MappedClass.Name, prop.Name, related.ObjectClass.Name});
					}
				}
			}
		}

		/// <summary>
		/// Тест наличия зависимости для ссылок на другие сущьности в класе.
		/// </summary>
		public virtual void DependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property prop, IDeleteRule related)
		{
			if(IgnoreProperties.ContainsKey(mapping.MappedClass) && IgnoreProperties[mapping.MappedClass].Any(x => x.PropertyName == prop.Name))
				Assert.Ignore(IgnoreProperties[mapping.MappedClass].First(x => x.PropertyName == prop.Name).ReasonForIgnoring);

			Assert.That(related.DeleteItems.Exists(r => r.ObjectClass == mapping.MappedClass && r.PropertyName == prop.Name)
						|| related.ClearItems.Exists(r => r.ObjectClass == mapping.MappedClass && r.PropertyName == prop.Name),
				"Для свойства {0}.{1} не определены зависимости удаления в классе {2}",
					mapping.MappedClass,
					prop.Name,
					related.ObjectClass.Name);
		}

		public static IEnumerable NhibernateMappedEntityRelationWithExistRuleCascadeRelated {
			get {
				Console.WriteLine("NhibernateMappedEntityRelationWithExistRuleCascadeRelated");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(m => !IgnoreMissingClass.ContainsKey(m.MappedClass))) {
					foreach(var prop in mapping.PropertyIterator.Where(p => p.IsEntityRelation)) {
						var related = DeleteConfig.ClassDeleteRules.OfType<IHibernateDeleteRule>().FirstOrDefault(c => c.ObjectClass == prop.Type.ReturnedClass);
						if(related == null || !related.IsRequiredCascadeDeletion )
							continue;
						yield return new TestCaseData(mapping, prop, related)
							.SetArgDisplayNames(new[] { mapping.MappedClass.Name, prop.Name, related.ObjectClass.Name });
					}
				}
			}
		}

		/// <summary>
		/// Тест наличия каскадной зависимости для ссылок на другие сущьности в класе.
		/// </summary>
		public virtual void CascadeDependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property prop, IDeleteRule related)
		{
			if(IgnoreProperties.ContainsKey(mapping.MappedClass) && IgnoreProperties[mapping.MappedClass].Any(x => x.PropertyName == prop.Name))
				Assert.Ignore(IgnoreProperties[mapping.MappedClass].First(x => x.PropertyName == prop.Name).ReasonForIgnoring);

			var info = DeleteConfig.GetDeleteRule(mapping.MappedClass);
			Assert.That(info.DeleteItems.Any(x => x.ParentPropertyName == prop.Name && x.IsCascade),
					"Cвойство {0}.{1} не имеет каскадного правила удаления, хотя класс {2} помечен как требующий каскадного удаления.",
					info.ObjectClass.Name,
					prop.Name,
					related.ObjectClass.Name);
		}

		#endregion

		#region Проверка коллекций Nhibernate на наличие правил удаления

		public static IEnumerable NhibernateMappedCollection {
			get {
				Console.WriteLine("NhibernateMappedCollection");
				foreach(var mapping in OrmConfig.NhConfig.ClassMappings.Where(m => !IgnoreMissingClass.ContainsKey(m.MappedClass))) {
					foreach(var prop in mapping.PropertyIterator.Where(p => p.Type.IsCollectionType)) {
						if(!(prop.Value is Bag))
							continue;
						var collectionMap = (prop.Value as Bag);
						if(collectionMap.IsInverse)
							continue;
						yield return new TestCaseData(mapping, prop)
							.SetArgDisplayNames(new[] { mapping.MappedClass.Name, prop.Name});
					}
				}
			}
		}

		//FIXME Лучше в будущем разделить тест на разные. Сейчас было лень переписывать код.
		public virtual void NHMappedCollectionsAllInOneTest(PersistentClass mapping, Property prop)
		{
			var collectionMap = (prop.Value as Bag);
			Type collectionItemType = null;
			if (collectionMap.Element is OneToMany)
				collectionItemType = (collectionMap.Element as OneToMany).AssociatedClass.MappedClass;
			else if (collectionMap.Element is ManyToOne)
				collectionItemType = (collectionMap.Element as ManyToOne).Type.ReturnedClass;

			var collectionItemClassInfo = DeleteConfig.GetDeleteRule(collectionItemType);

			Assert.That(collectionItemClassInfo, Is.Not.Null,
				"Класс {0} не имеет правил удаления, но используется в коллекции {1}.{2}.",
				collectionItemType,
				mapping.MappedClass.Name,
				prop.Name);

			var info = DeleteConfig.GetDeleteRule(mapping.MappedClass);
			Assert.That(info, Is.Not.Null,
					"Коллекция {0}.{1} объектов {2} замаплена в ненастроенном классе.",
					mapping.MappedClass.Name,
					prop.Name,
					collectionItemType.Name);

			if(collectionMap.IsOneToMany)
			{
				var deleteDepend = info.DeleteItems.Find(r => r.ObjectClass == collectionItemType && r.CollectionName == prop.Name);
				Assert.That(deleteDepend, Is.Not.Null,
				            "Для коллекции {0}.{1} не определены зависимости удаления класса {2}",
				            mapping.MappedClass.Name,
				            prop.Name,
				            collectionItemType.Name
				           );
			}
			else
			{
				var removeDepend = collectionItemClassInfo.RemoveFromItems.Find(r => r.ObjectClass == info.ObjectClass && r.CollectionName == prop.Name);
				Assert.That(removeDepend, Is.Not.Null,
							"Для коллекции {0}.{1} не определены зависимости удаления элементов при удалении класса {2}",
							mapping.MappedClass.Name,
							prop.Name,
							collectionItemType.Name
						   );
			}

			if (collectionItemClassInfo is IHibernateDeleteRule) {
				Assert.That(info, Is.InstanceOf<IHibernateDeleteRule>(),
						"Удаление через Hibernate элементов коллекции {0}.{1}, поддерживается только если удаление родительского класса {0} настроено тоже через Hibernate",
						mapping.MappedClass.Name,
						prop.Name
					);
			}
		}

		#endregion

		#region Проверка классов на наличие свойств Name или Title, для отображения объектов в диалогах удаления.

		public static IEnumerable AllDeleteRules {
			get {
				Console.WriteLine("AllDeleteRules");
				foreach(var info in DeleteConfig.ClassDeleteRules) {
					yield return new TestCaseData(info)
							.SetDescription("Проверка правил удаления на наличие Name или Tilte")
							.SetArgDisplayNames(new[] { info.ObjectClass.Name });
				}
			}
		}

		public virtual void DeleteRules_ExistTitle_Test(IDeleteRule info)
		{
			var prop = info.ObjectClass.GetProperty("Title");
			if(prop != null) {
				Assert.Pass("Найдено свойство Title");
			}

			prop = info.ObjectClass.GetProperty("Name");
			if(prop != null) {
				Assert.Pass("Найдено свойство Name");
			}

			if(info.ObjectClass.GetMethod("ToString", new Type[] { }).DeclaringType == info.ObjectClass) {
				Assert.Pass("В классе переопределено свойство ToString");
			}

			Assert.Fail($"В классе {info.ObjectClass}, нет свойств Title или Name. Что не позволяет при удалении красиво вывести пользователю информациию об удаляемом объекте.");
		}

		#endregion
	}
}
