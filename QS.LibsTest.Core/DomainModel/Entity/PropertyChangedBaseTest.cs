using System.Collections.Generic;
using NUnit.Framework;
using QS.DomainModel.Entity;
using QS.Test.TestApp.Domain.Entity;

namespace QS.Test.DomainModel.Entity
{
	[TestFixture(TestOf = (typeof(PropertyChangedBase)))]
	public class PropertyChangedBaseTest
	{
		[Test(Description = "Проверяем что можем вызвать событие обновленя у свойства которого нет.")]
		public void OnPropertyChanged_NotExistPropertyTest()
		{
			var firedList = new List<string>();
			var item = new PropertyChangedClass();
			item.PropertyChanged += (sender, args) => firedList.Add(args.PropertyName);
			item.ManualInternalFire("NotExistProperty");
			Assert.That(firedList.Count, Is.EqualTo(1));
			Assert.That(firedList[0], Is.EqualTo("NotExistProperty"));
		}
		
		[Test(Description = "Проверяем что вызваем событие обновленя у свойств указанных в атрибуте PropertyChangedAlso.")]
		public void Property_PropertyChangedAlsoTest()
		{
			var firedList = new List<string>();
			var item = new PropertyChangedClass();
			item.PropertyChanged += (sender, args) => firedList.Add(args.PropertyName);
			item.Property1 = "Fire!!";
			Assert.That(firedList.Count, Is.EqualTo(3));
			Assert.That(firedList.Contains(nameof(item.Property1)), Is.True);
			Assert.That(firedList.Contains("NotExistProperty"), Is.True);
			Assert.That(firedList.Contains(nameof(item.Property2)), Is.True);
		}
		
		[Test(Description = "Случай с override, текущая реализация не проверяет атрибуты базового свойства, возможно это не правильно.")]
		public void OnPropertyChanged_OverridePropertyTest()
		{
			var firedList = new List<string>();
			var item = new PropertyChangedDescendantClass();
			item.PropertyChanged += (sender, args) => firedList.Add(args.PropertyName);
			item.ManualInternalFire(nameof(item.Property1));
			Assert.That(firedList.Count, Is.EqualTo(2)); //Возможно было бы адекватнее 4 но сейчас такая реализация. 
			Assert.That(firedList.Contains(nameof(item.Property1)), Is.True);
			Assert.That(firedList.Contains(nameof(item.Property3)), Is.True);
			// Возможно было бы адекватно если бы атрибуты базовых свойств тоже читались. Но сейчас работает так.
			// Assert.That(firedList.Contains("NotExistProperty"), Is.True);
			// Assert.That(firedList.Contains(nameof(item.Property2)), Is.True);
		}
		
		[Test(Description = "Проверяем что атрибут срабабывает в том числе и на скрытых свойствах. " +
		                    "Реальный кейс появление ошибки System.Reflection.AmbiguousMatchException : Ambiguous match found.")]
		public void OnPropertyChanged_NewPropertyTest()
		{
			var firedList = new List<string>();
			var item = new PropertyChangedDescendantClass2();
			item.PropertyChanged += (sender, args) => firedList.Add(args.PropertyName);
			item.ManualInternalFire(nameof(item.Property1));
			Assert.That(firedList.Count, Is.EqualTo(4)); //2 первых тоже было бы адекватно ибо тест  OnPropertyChanged_OverridePropertyTest не вызывает атрибуты из базового класса.
			// Фишка этого случая в том что свойство Property1 в наследнике и базовом классе имеют разный тип, поэтому в рефлексии их два, в отличии от случая с override 
			Assert.That(firedList.Contains(nameof(item.Property1)), Is.True);
			Assert.That(firedList.Contains(nameof(item.Property3)), Is.True);
			//Возможно не обязательно должны срабатывать свойства ниже
			Assert.That(firedList.Contains("NotExistProperty"), Is.True);
			Assert.That(firedList.Contains(nameof(item.Property2)), Is.True);
		}
		
		[Test(Description = "Проверяем что атрибут PropertyChangedAlso срабатывает если находится в базовом классе")]
		public void OnPropertyChanged_OnBasePropertyTest()
		{
			var firedList = new List<string>();
			var item = new PropertyChangedDescendantClass();
			item.PropertyChanged += (sender, args) => firedList.Add(args.PropertyName);
			item.Property10 = true;
			Assert.That(firedList.Count, Is.EqualTo(2));
			Assert.That(firedList.Contains(nameof(item.Property10)), Is.True);
			Assert.That(firedList.Contains(nameof(item.Property2)), Is.True);
		}
	}
}