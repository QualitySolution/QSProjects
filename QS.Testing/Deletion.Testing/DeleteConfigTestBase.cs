using System;
using System.Collections;
using NUnit.Framework;

namespace QS.Deletion.Testing
{
	public abstract class DeleteConfigTestBase
	{
		public static IEnumerable AllDeleteItems {
			get {
				Console.WriteLine("AllDeleteItems");
				foreach(var info in DeleteConfig.ClassDeleteRules) {
					foreach(var item in info.DeleteItems) {
						yield return new TestCaseData(info, item);
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
	}
}
